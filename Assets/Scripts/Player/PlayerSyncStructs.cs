using UnityEngine;
using System.Collections.Generic;

namespace Bangboom.Player
{
    public class PlayerSyncStructs
    {
        public struct Inputs
        {
            public int submitButon;
            public Vector2 stickMove;
            public Vector3 mousePos;

        }

        public struct InputMessage
        {
            public float deliveryTime;
            public uint startTickNumber;
            public Vector3 position;
            public List<Inputs> inputs;
        }

        public struct ClientState
        {
            public Vector2 position;
            public float rotation;
            public Vector3 velocity;
        }

        public struct StateMessage
        {
            public float deliveryTime;
            public uint tickNumber;
            public Vector2 position;
            public float rotation;
            public Vector3 velocity;
            public float angularVelocity;
        }
    }
}