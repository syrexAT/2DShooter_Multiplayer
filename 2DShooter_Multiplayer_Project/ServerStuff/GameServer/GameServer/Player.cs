using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        public int id;
        public string username;

        public Vector3 postion;
        public Quaternion rotation;

        //since it will be applied every tick, divide it by ticks per second, is the same as multiplying by time.deltatime
        private float moveSpeed = 5f / Constants.TICKS_PER_SEC;
        private bool[] inputs;

        public Player(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            username = _username;
            postion = _spawnPosition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 _inputDirection = Vector2.Zero;
            if (inputs[0])
            {
                _inputDirection.Y += 1;
            }

            if (inputs[1])
            {
                _inputDirection.Y -= 1;
            }

            if (inputs[2])
            {
                _inputDirection.X += 1;
            }

            if (inputs[3])
            {
                _inputDirection.X -= 1;
            }

            Move(_inputDirection);
        }

        private void Move(Vector2 _inputDirection)
        {
            Vector3 _forward = Vector3.Transform(new Vector3(0, 1, 0), rotation);
            Vector3 _right = Vector3.Transform(new Vector3(-1, 0, 0), rotation);

            Vector3 _moveDirection = _right * _inputDirection.X + _forward * _inputDirection.Y;
            postion += _moveDirection * moveSpeed;

            //send player pos and rot player packet
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        public void SetInput(bool[] _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }
    }
}
