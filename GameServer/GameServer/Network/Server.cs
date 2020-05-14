using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer.Network
{
    public class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int fromClient, Packet packet);

        public static Dictionary<int, PacketHandler> PacketHandlers;

        private static TcpListener _tcpListener;

        private static UdpClient _udpListener;

        public static void Start(int maxPlayers)
        {
            MaxPlayers = maxPlayers;
            Port = Constants.RUNNING_PORT;

            Console.WriteLine("Starting Server...");

            InitializeServerData();

            _tcpListener = new TcpListener(Constants.RUNNING_PORT);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

            _udpListener = new UdpClient(Port);
            _udpListener.BeginReceive(UdpReceiveCallback, null);

            Console.WriteLine($"Server started on {Port}.");
        }

        private static void TcpConnectCallback(IAsyncResult result)
        {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (Clients[i].Tcp.Socket == null)
                {
                    Clients[i].Tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server is Full!");
        }

        private static void UdpReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);
                _udpListener.BeginReceive(UdpReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();
                    if (clientId == 0)
                    {
                        return;
                    }

                    if (Clients[clientId].Udp.EndPoint == null)
                    {
                        Clients[clientId].Udp.Connect(clientEndPoint);
                        return;
                    }

                    if (Clients[clientId].Udp.EndPoint.ToString() == clientEndPoint.ToString())
                    {
                        Clients[clientId].Udp.HandleData(packet);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error receiving UDP data: {e}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {e}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }

            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int) ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
                {(int) ClientPackets.playerMovement, ServerHandle.PlayerMovement}
            };
            Console.WriteLine("Initialized Server Data!");
        }
    }
}