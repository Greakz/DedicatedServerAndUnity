using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;
    public static int DataBufferSize = 4096;

    public string Ip = "127.0.01";
    public int Port = 26950;
    public int MyId = 0;
    public TCP Tcp;
    public UDP Udp;

    private bool _isConnected = false;

    private delegate void PacketHandler(Packet packet);

    private static Dictionary<int, PacketHandler> PacketHandlers;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log("Networking Client already exists, destroying overload!");
            Destroy(this);
        }
    }

    private void Start()
    {
        Tcp = new TCP();
        Udp = new UDP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }


    public void ConnectToServer()
    {
        _isConnected = true;
        InitializeClientData();
        Tcp.Connect();
    }

    public class TCP
    {
        public TcpClient Socket;
        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;

        public void Connect()
        {
            Socket = new TcpClient
            {
                ReceiveBufferSize = DataBufferSize,
                SendBufferSize = DataBufferSize
            };
            _receiveBuffer = new byte[DataBufferSize];
            Socket.BeginConnect(Instance.Ip, Instance.Port, ConnectCallback, Socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            if (!Socket.Connected)
            {
                return;
            }

            _stream = Socket.GetStream();

            _receivedData = new Packet();

            _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
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
                Debug.Log($"Error sending data to server via TCP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                // Todo: Handle Data;
                _receivedData.Reset(HandleData(data));
                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Disconnect();
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
                        PacketHandlers[packetId](packet);
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

        private void Disconnect()
        {
            Instance.Disconnect();

            _stream = null;
            _receiveBuffer = null;
            _receivedData = null;
            Socket = null;
        }
    }

    public class UDP
    {
        public UdpClient Socket;
        public IPEndPoint EndPoint;

        public UDP()
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(Instance.Ip), Instance.Port);
        }

        public void Connect(int localPort)
        {
            Socket = new UdpClient(localPort);
            
            Socket.Connect(EndPoint);

            Socket.BeginReceive(ReceiveCallback, null);

            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(Instance.MyId);
                if (Socket != null)
                {
                    Socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to server via UDP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = Socket.EndReceive(result, ref EndPoint);
                Socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    Instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using (Packet packet = new Packet(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetId = packet.ReadInt();
                    PacketHandlers[packetId](packet);
                }
            });
        }
        
        private void Disconnect()
        {
            Instance.Disconnect();

            EndPoint = null;
            Socket = null;
        }
    }
    
    private void InitializeClientData()
    {
        PacketHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int) ServerPackets.welcome, ClientHandle.Welcome},
            {(int) ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer},
            {(int) ServerPackets.playerPosition, ClientHandle.PlayerPosition},
            {(int) ServerPackets.playerRotation, ClientHandle.PlayerRotation},
        };
        Debug.Log("Initialised Packets!");
    }
    
    private void Disconnect()
    {
        if (_isConnected)
        {
            _isConnected = false;
            Tcp.Socket.Close();
            Udp.Socket.Close();
            
            Debug.Log("Disconnected from Server.");
        }
    }
}