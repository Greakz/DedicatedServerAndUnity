using System;
using System.Numerics;

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
            Server.Clients[fromClient].SendIntoGame(userName);
        }

        public static void PlayerMovement(int fromClient, Packet packet)
        {
            bool[] inputs = new bool[packet.ReadInt()];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = packet.ReadBool();
            }

            Quaternion rotation = packet.ReadQuaternion();

            Server.Clients[fromClient].Player.SetInput(inputs, rotation);
        }
    }
}