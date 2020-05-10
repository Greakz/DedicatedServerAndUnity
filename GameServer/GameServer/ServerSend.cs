using System;

namespace GameServer
{
    public class ServerSend
    {
        
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.Clients[toClient].Tcp.SendData(packet);
        }
        
        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.Clients[toClient].Udp.SendData(packet);
        }

        private static void SendTCPToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].Tcp.SendData(packet);   
            }
        }

        private static void SendUDPToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].Udp.SendData(packet);   
            }
        }

        private static void SendTCPToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.Clients[i].Tcp.SendData(packet);
                }
            }
        }
        
        private static void SendUDPToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.Clients[i].Udp.SendData(packet);
                }
            }
        }
        
        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int) ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }
        
        public static void UDPTest(int toClient)
        {
            using (Packet packet = new Packet((int) ServerPackets.udpTest))
            {
                packet.Write("A test package for UDP");
                SendUDPData(toClient, packet);
            }
        }
    }
}