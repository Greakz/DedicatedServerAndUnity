using System;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public class Client
    {
        public const int DataBufferSize = 4096;

        public int Id;
        public TCP Tcp;
        public UDP Udp;

        public Client(int clientId)
        {
            Id = clientId;
            Tcp = new TCP(Id);
            Udp = new UDP(Id);
        }

        public class TCP
        {
            public TcpClient Socket;

            private readonly int _id;
            private NetworkStream _stream;
            private Packet _receivedData;
            private byte[] _receiveBuffer;

            public TCP(int id)
            {
                _id = id;
            }

            public void Connect(TcpClient socket)
            {
                Socket = socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                _stream = socket.GetStream();
                
                _receiveBuffer = new byte[DataBufferSize];
                _receivedData = new Packet();
                
                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                
                ServerSend.Welcome(_id, "Welcome to the Server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error Sending Data To Player {_id} via TCP:");
                    Console.WriteLine(e);
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = _stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        // Todo: Disconnect;
                        return;
                    }
                    
                    byte[] data = new byte[byteLength];
                    Array.Copy(_receiveBuffer, data, byteLength);
                    
                    _receivedData.Reset(HandleData(data));
                    // Todo: Handle Data;
                    _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Todo: Disconnect Client
                }
            }
            
            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                _receivedData.SetBytes(data);
                if (_receivedData.UnreadLength() >= 4)
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
                {
                    byte[] packetBytes = _receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Server.PacketHandlers[packetId](_id ,packet);
                        }
                    });

                    packetLength = 0;
                    if (_receivedData.UnreadLength() >= 4)
                    {
                        packetLength = _receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public class UDP
        {
            public IPEndPoint EndPoint;
            
            private int Id;

            public UDP(int id)
            {
                Id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                EndPoint = endPoint;
                ServerSend.UDPTest(Id);
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(EndPoint, packet);
            }

            public void HandleData(Packet packetData)
            {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);
                
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.PacketHandlers[packetId](Id, packet);

                    }
                });
            }
        }
    }
}