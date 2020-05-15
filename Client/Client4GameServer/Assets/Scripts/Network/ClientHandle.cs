using System;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.Instance.MyId = myId;
        ClientSend.WelcomeReceived();

        Client.Instance.Udp.Connect(((IPEndPoint) Client.Instance.Tcp.Socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.Instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        bool isMoving = packet.ReadBool();

        GameManager.Players[id].TargetPosition = position;
        GameManager.Players[id].ReachTargetTime = DateTime.Now.AddMilliseconds(1000f / 20f); // server running at 20 ticks per sec
        GameManager.Players[id].setMoving(isMoving);
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.Players[id].transform.rotation = rotation;
    }
}