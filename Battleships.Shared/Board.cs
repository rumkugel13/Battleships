using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Battleships.Shared
{
    public struct Board
    {
        public static int Size = 10;
        public static int TileSize = 31;

        public Vector2 LocalPosition;
        public Rectangle MouseArea;
        public Ship[] Ships;
        public Dictionary<Point, TileState> Tiles;

        public enum TileState
        {
            Unknown,
            Miss,
            Hit
        }

        public Vector2 ScreenToGrid(Vector2 point)
        {
            return (point - LocalPosition) / TileSize;
        }

        public Vector2 GridToScreen(Vector2 point)
        {
            return (point * TileSize) + LocalPosition;
        }

        public Vector2 SnapToGrid(Vector2 point)
        {
            point -= LocalPosition;
            point = point - new Vector2(point.X % TileSize, point.Y % TileSize);
            point += LocalPosition;
            return point;
        }

        public bool IsMouseOver(Vector2 position)
        {
            return MouseArea.Contains(position);
        }

        public void SetMouseAreaFromSpriteBounds(Rectangle bounds)
        {
            Rectangle rect = bounds;
            rect.Location = LocalPosition.ToPoint();
            rect.Size -= new Point(TileSize + 1, TileSize + 1);
            rect.Offset(TileSize, TileSize);
            MouseArea = rect;
        }

        // todo: allow access using numbers/letters eg F7
        public TileState GetTileState(Point pos)
        {
            if (Tiles.TryGetValue(pos, out TileState state))
            {
                return state;
            }

            return TileState.Unknown;
        }

        public void SetTileState(Point pos, TileState state)
        {
            Tiles[pos] = state;
        }

        public int GetHitCount()
        {
            int result = 0;
            foreach (TileState tile in Tiles.Values)
            {
                if (tile == TileState.Hit)
                    result++;
            }
            return result;
        }

        public int GetShipArea()
        {
            int result = 0;
            foreach (Ship ship in Ships)
                result += ship.GetLength();
            return result;
        }

        public bool AllShipsHit()
        {
            return GetShipArea() == GetHitCount();
        }

        public bool AllTilesSet()
        {
            return Tiles.Count == Size * Size;
        }

        public bool DoAnyShipsOverlap()
        {
            for (int i = 0; i < this.Ships.Length; i++)
            {
                Ship ship1 = this.Ships[i];
                for (int j = i + 1; j < this.Ships.Length; j++)
                {
                    Ship ship2 = this.Ships[j];
                    if (ship1.DoesOverlapWith(ship2))
                        return true;
                }
            }

            return false;
        }

        public void PlaceShipsRandom(Random random)
        {
            List<Ship> ships = new List<Ship>();
            for (int i = 0; i < 5; i++)
            {
                Ship ship = new Ship();
                do
                {
                    ship.Type = (Ship.ShipType)i;
                    ship.Rotation = random.Next(4);

                    if (ship.Rotation % 2 == 0)
                    {
                        ship.Position.X = random.Next(1, 11);
                        ship.Position.Y = random.Next(1, 11 - ship.GetLength());
                    }
                    else
                    {
                        ship.Position.X = random.Next(1, 11 - ship.GetLength());
                        ship.Position.Y = random.Next(1, 11);
                    }
                }
                while (ship.DoesShipOverlapAny(ships.ToArray()));

                ships.Add(ship);
            }
            Ships = ships.ToArray();
            Tiles.Clear();
        }

        public TileState GetMissileHitResult(Point point)
        {
            foreach (Ship ship in Ships)
            {
                if (ship.Rotation % 2 == 0)
                {
                    if (point.X == ship.Position.X && point.Y >= ship.Position.Y && point.Y < ship.Position.Y + ship.GetLength())
                        return TileState.Hit;
                }
                else
                {
                    if (point.Y == ship.Position.Y && point.X >= ship.Position.X && point.X < ship.Position.X + ship.GetLength())
                        return TileState.Hit;
                }
            }

            return TileState.Miss;
        }
    }
}
