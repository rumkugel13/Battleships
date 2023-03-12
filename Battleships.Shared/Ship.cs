using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Battleships.Shared
{
    public struct Ship
    {
        public Point Position;
        public ShipType Type;
        public int Rotation;
        private static int[] Lengths = new int[] { 2, 3, 3, 4, 5 };

        public enum ShipType
        {
            PatrolBoat,
            Cruiser,
            Submarine,
            Battleship,
            AircraftCarrier
        }

        public int GetSheetIndex()
        {
            return Rotation * 5 + (int)Type;
        }

        public int GetLength()
        {
            return Lengths[(int)Type];
        }

        public Rectangle GetRect()
        {
            return new Rectangle(Position, new Point(this.Rotation % 2 != 0 ? GetLength() : 1, this.Rotation % 2 == 0 ? GetLength() : 1));
        }

        public bool DoesOverlapWith(Ship ship2)
        {
            // ships can touch
            return GetRect().Intersects(ship2.GetRect());

            // ships cannot touch
            //Rectangle temp = GetRect();
            //temp.Inflate(1, 1);
            //return temp.Intersects(ship2.GetRect());
        }

        public bool DoesShipOverlapAny(Ship[] ships)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                Ship ship = ships[i];
                if (DoesOverlapWith(ship))
                    return true;
            }

            return false;
        }
    }
}
