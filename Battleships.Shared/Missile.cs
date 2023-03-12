using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Battleships.Shared
{
    public struct Missile
    {
        public Vector2 Position;
        public Vector2 Target;
        public MissileState State;
        public float Velocity;

        public enum MissileState
        {
            Launch,
            Airborne,
            Impact
        }

        public float Rotation()
        {
            return (float)Math.Atan2(Target.Y - Position.Y, Target.X - Position.X);
        }

        public Vector2 Direction()
        {
            return Target - Position;
        }
    }
}