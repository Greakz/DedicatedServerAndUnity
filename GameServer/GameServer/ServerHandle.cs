using System;

namespace GameServer
{
    public class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            string userName = packet.ReadString();
            
            Console.WriteLine($"{Server.Clients[fromClient].Tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient} [{userName}].");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player \"{userName}\" [Id: {fromClient} has assumed the wrong Client ID ({clientIdCheck}) ");
            }
            // Todo: Send Player into Game!
        }

        public static void UDPTestReceived(int fromClient, Packet packet)
        {
            string msg = packet.ReadString();
            Console.WriteLine($"Received Packet via UDP: {msg}");
        }
    }
}