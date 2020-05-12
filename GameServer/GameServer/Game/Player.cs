using System;
using System.Numerics;
using GameServer.Properties;

namespace GameServer
{
    public class Player
    {
        public int Id;
        public string Username;

        public Vector3 Position;
        public Quaternion Rotation;

        private float MoveSpeed = 5f / Constants.TICKS_PER_SEC;
        private bool[] Inputs;
        private float FallSpeed = 1.0f;

        public Player(int id, string username, Vector3 spawnPosition)
        {
            Id = id;
            Username = username;
            Position = spawnPosition;
            Rotation = Quaternion.Identity;

            Inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 inputDirection = Vector2.Zero;
            if (Inputs[0])
            {
                inputDirection.Y += 1;
            }

            if (Inputs[1])
            {
                inputDirection.Y -= 1;
            }

            if (Inputs[2])
            {
                inputDirection.X += 1;
            }

            if (Inputs[3])
            {
                inputDirection.X -= 1;
            }

            Move(inputDirection);
        }

        private void Move(Vector2 inputDirection)
        {

            float distanceFromCenter = Position.Length();
            if (distanceFromCenter > 5.0f)
            {
                // only vertical fall, left platformer
                Position += -Vector3.UnitY * FallSpeed;
                FallSpeed *= 1.05f;
                if (Position.Y < -20)
                {
                    Position = Vector3.Zero;
                    FallSpeed = 1.0f;
                }
            }
            else
            {
                // move normal
                Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), Rotation);
                Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

                Vector3 moveDirection = right * inputDirection.X + forward * inputDirection.Y;
                Position += moveDirection * MoveSpeed;
            }

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }
        
        public void SetInput(bool[] inputs, Quaternion rotation)
        {
            Inputs = inputs;
            Rotation = rotation;
        }
    }
}