using UnityEngine;
using System.Collections.Generic;

namespace Bangboom.Player
{
    public static class PlayerSyncStructs
    {
        public struct Inputs
        {
            public int SubmitButton;
            public Vector2 MovementDirection;
            public Vector3 MousePos;
        }

        public struct InputMessage
        {
            public float DeliveryTime;
            public uint StartTickNumber;
            public Vector3 Position;
            public List<Inputs> Inputs;
        }

        public struct ClientState
        {
            public Vector2 Position;
            public float Rotation;
            public Vector3 Velocity;
        }

        public struct StateMessage
        {
            public float DeliveryTime;
            public uint TickNumber;
            public Vector2 Position;
            public float Rotation;
            public Vector3 Velocity;
            public float AngularVelocity;
        }
    }
}