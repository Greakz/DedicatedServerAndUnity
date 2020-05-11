namespace GameServer
{
    public class GameLogic
    {
        public static void Update()
        {
            foreach (Client client in Server.Clients.Values)
            {
                if (client.Player != null)
                {
                    client.Player.Update();
                }
            }


            ThreadManager.UpdateMain();
        }
    }
}