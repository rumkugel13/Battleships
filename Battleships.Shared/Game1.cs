using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Battleships.Shared
{
    using TileState = Board.TileState;
    using ShipType = Ship.ShipType;

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D tokens, shipGrid, oceanGrid, radarGrid, panel, radarAnimation, radarAnimGrid, blipAnimation, missileTexture;
        private SpriteFont font18, font24;
        private List<Rectangle> tokenSheet, shipSheet;
        private Dictionary<string, Rectangle> radarSheet, blipSheet, missileSheet;
        private Vector2 tokenPos;
        private Ship ship1;
        private int selectedIndex = -1;
        private Board oceanBoard, radarBoard;
        private KeyboardState oldKeyboardState, keyboardState;
        private MouseState oldMouseState, mouseState;
        private int token;
        private string shipInside = "false", shipOverlap = "false", anyOverlap = "false";
        private Random random;
        private GameState gameState, nextGameState;
        private string statusText;
        private bool player1Won;
        private float radarAnimationTime, radarAnimationDuration, blipAnimationTime, blipAnimationDuration;
        private bool blipVisible, radarShipsVisible, missileVisible;
        private Vector2 blipLocation;
        private Missile missile;

        enum GameState
        {
            Init, Prepare, Ready, Player1, Player2, End
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            radarBoard = new Board();
            oceanBoard = new Board();
            radarShipsVisible = false;

            random = new Random();
            // to trigger transition
            gameState = GameState.Init;
            nextGameState = GameState.Ready;

            // calls loadcontent
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            panel = Content.Load<Texture2D>("Textures/metalic_panel");

            tokens = Content.Load<Texture2D>("Textures/Tokens");
            tokenSheet = Content.Load<List<Rectangle>>("Spritesheets/Tokens");

            shipGrid = Content.Load<Texture2D>("Textures/BattleShipSheet");
            shipSheet = Content.Load<List<Rectangle>>("Spritesheets/BattleShipSheet");

            radarSheet = Content.Load<Dictionary<string, Rectangle>>("Spritesheets/radar_base_sheet");
            radarAnimation = Content.Load<Texture2D>("Textures/radar_base_sheet");
            radarAnimationDuration = 10; //seconds
            radarAnimationTime = 0f;
            radarAnimGrid = Content.Load<Texture2D>("Textures/Grid_faint");

            blipSheet = Content.Load<Dictionary<string, Rectangle>>("Spritesheets/radar_blip_sheet");
            blipAnimation = Content.Load<Texture2D>("Textures/radar_blip_sheet");
            blipAnimationDuration = 1; //seconds
            blipAnimationTime = 0f;
            blipVisible = false;

            radarGrid = Content.Load<Texture2D>("Textures/Radargrid");
            radarBoard.LocalPosition = new(40, 40);
            radarBoard.SetMouseAreaFromSpriteBounds(radarGrid.Bounds);
            oceanGrid = Content.Load<Texture2D>("Textures/Oceangrid");
            oceanBoard.LocalPosition = new(GraphicsDevice.Viewport.Width - oceanGrid.Width - 40, 40);
            oceanBoard.SetMouseAreaFromSpriteBounds(oceanGrid.Bounds);

            missileTexture = Content.Load<Texture2D>("Textures/projectile_rocket_16x16");
            missileSheet = Content.Load<Dictionary<string, Rectangle>>("Spritesheets/projectile_rocket_16x16");
            missile.Position = oceanBoard.LocalPosition;
            missile.Target = MousePosition();
            missile.Velocity = 300f;

            token = 0;

            ship1 = new Ship();
            ship1.Position = new(300, 300);
            ship1.Rotation = 2;
            ship1.Type = ShipType.Submarine;

            oceanBoard.Tiles = new Dictionary<Point, TileState>();
            oceanBoard.Ships = new Ship[5];
            oceanBoard.PlaceShipsRandom(random);

            radarBoard.Tiles = new Dictionary<Point, TileState>();
            radarBoard.Ships = new Ship[5];
            radarBoard.PlaceShipsRandom(random);

            font18 = Content.Load<SpriteFont>("Fonts/USN_Stencil_18");
            font24 = Content.Load<SpriteFont>("Fonts/USN_Stencil_24");
        }

        //void PlaceShips()
        //{
        //    oceanBoard.Ships[0] = new Ship();
        //    oceanBoard.Ships[0].Type = (ShipType)0;
        //    oceanBoard.Ships[0].Rotation = 1;
        //    oceanBoard.Ships[0].Position = new Point(1, 1);

        //    oceanBoard.Ships[1] = new Ship();
        //    oceanBoard.Ships[1].Type = (ShipType)1;
        //    oceanBoard.Ships[1].Rotation = 2;
        //    oceanBoard.Ships[1].Position = new Point(5, 1);

        //    oceanBoard.Ships[2] = new Ship();
        //    oceanBoard.Ships[2].Type = (ShipType)2;
        //    oceanBoard.Ships[2].Rotation = 3;
        //    oceanBoard.Ships[2].Position = new Point(3, 7);

        //    oceanBoard.Ships[3] = new Ship();
        //    oceanBoard.Ships[3].Type = (ShipType)3;
        //    oceanBoard.Ships[3].Rotation = 0;
        //    oceanBoard.Ships[3].Position = new Point(8, 2);

        //    oceanBoard.Ships[4] = new Ship();
        //    oceanBoard.Ships[4].Type = (ShipType)4;
        //    oceanBoard.Ships[4].Rotation = 1;
        //    oceanBoard.Ships[4].Position = new Point(3, 9);
        //}

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // updates components
            base.Update(gameTime);

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            TransitionToNextState();

            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            // todo: rules: shoot until water / one shot per round / 3 shots per round
            //  ships can touch diag / ships can touch all around / ships cannot touch

            switch (gameState)
            {
                case GameState.Init:
                    break;
                case GameState.Prepare:
                    // todo: drag and drop ships to place them, can still move rotate after drop
                    // todo: once ships placed, can still move them but change text on statuslabel to "hit spacebar to start" or smth
                    //SelectShip();
                    break;
                case GameState.Ready:
                    if (KeyJustPressed(Keys.Space))
                    {
                        SwitchGameState(GameState.Player1);
                    }

                    if (KeyJustPressed(Keys.R))
                    {
                        radarBoard.PlaceShipsRandom(random);
                        oceanBoard.PlaceShipsRandom(random);
                    }
                    break;
                case GameState.Player1:
                    // todo: select tile with arrow keys / type coordinate eg f7 / mouse cursor
                    //   shoot with spacebar / left mouse click
                    switch (missile.State)
                    {
                        case Missile.MissileState.Launch:
                            Shoot();
                            break;
                        case Missile.MissileState.Airborne:
                            MissileFly(elapsedSeconds);
                            break;
                        case Missile.MissileState.Impact:
                            MissileImpact(ref radarBoard, GameState.Player2);
                            break;
                    }

                    if (radarBoard.AllShipsHit())
                    {
                        SwitchGameState(GameState.End);
                        player1Won = true;
                    }
                    break;
                case GameState.Player2:
                    // todo: better enemy, which shoots around hit target

                    switch (missile.State)
                    {
                        case Missile.MissileState.Launch:
                            ShootRandom(ref oceanBoard);
                            break;
                        case Missile.MissileState.Airborne:
                            MissileFly(elapsedSeconds);
                            break;
                        case Missile.MissileState.Impact:
                            MissileImpact(ref oceanBoard, GameState.Player1);
                            break;
                    }

                    if (oceanBoard.AllShipsHit())
                    {
                        SwitchGameState(GameState.End);
                        player1Won = false;
                    }

                    break;
                case GameState.End:
                    if (KeyJustPressed(Keys.R))
                    {
                        SwitchGameState(GameState.Ready);
                        oceanBoard.Tiles.Clear();
                        radarBoard.Tiles.Clear();
                    }
                    break;
            }

            //MouseMoveTarget();

            //ShipSelectTypeAndRotation();

            //MouseMoveShip();

            //token = MouseOverShip() ? 1 : 0;

            //PlaceShip();

            //anyOverlap = oceanBoard.DoAnyShipsOverlap().ToString();

            if (blipVisible)
            {
                blipAnimationTime += elapsedSeconds;
                if (blipAnimationTime > blipAnimationDuration)
                {
                    blipVisible = false;
                }
            }

            if (radarBoard.IsMouseOver(MousePosition()) && mouseState.LeftButton.Equals(ButtonState.Pressed) && oldMouseState.LeftButton.Equals(ButtonState.Released))
            {
                blipVisible = true;
                blipAnimationTime = 0;
                blipLocation = MousePosition();
            }

            radarAnimationTime += elapsedSeconds;
            if (radarAnimationTime > radarAnimationDuration)
                radarAnimationTime -= radarAnimationDuration;

            ManuallySwitchGameState();

            oldKeyboardState = keyboardState;
            oldMouseState = mouseState;
        }

        private Vector2 MousePosition()
        {
            return new(mouseState.X, mouseState.Y);
        }

        private static int Modulo(int number, int N)
        {
            return (number % N + N) % N;
        }

        private bool KeyJustPressed(Keys key)
        {
            return oldKeyboardState[key] == KeyState.Up && keyboardState[key] == KeyState.Down;
        }

        private bool KeyJustReleased(Keys key)
        {
            return oldKeyboardState[key] == KeyState.Down && keyboardState[key] == KeyState.Up;
        }

        private void SwitchGameState(GameState state)
        {
            nextGameState = state;
        }

        private void ManuallySwitchGameState()
        {
            if (KeyJustReleased(Keys.N))
            {
                SwitchGameState((GameState)((int)(gameState + 1) % (int)(GameState.End + 1)));
            }

            if (KeyJustReleased(Keys.B))
            {
                SwitchGameState((GameState)(Modulo((int)(gameState - 1),(int)(GameState.End + 1))));
            }
        }

        private void TransitionToNextState()
        {
            if (nextGameState != gameState)
            {
                gameState = nextGameState;

                switch (gameState)
                {
                    case GameState.Init:
                        break;
                    case GameState.Prepare:
                        statusText = "Place your ships";
                        // todo: allow on new game
                        //PlaceShipsRandom(ref radarBoard);
                        break;
                    case GameState.Ready:
                        statusText = "Hit spacebar to start";
                        break;
                    case GameState.Player1:
                        statusText = "Your turn";
                        break;
                    case GameState.Player2:
                        statusText = "Enemy turn";
                        break;
                    case GameState.End:
                        statusText = "Game over. You " + (player1Won ? "won" : "lost") + ". Press R to Retry";
                        break;
                }
            }
        }

        //private void PlaceShip()
        //{
        //    if (KeyJustPressed(Keys.Enter) && !DoesShipOverlapAny(oceanBoard.Ships, ship1))
        //    {
        //        List<Ship> ships = new List<Ship>(oceanBoard.Ships);
        //        ships.Add(ship1);
        //        oceanBoard.Ships = ships.ToArray();
        //        ship1 = new Ship();
        //    }
        //}

        private void ShootRandom(ref Board target)
        {
            if (!target.AllTilesSet())
            {
                // todo: refactor by precalculating possible points into array, shuffling it and picking one out
                Point point;
                do
                {
                    point = new(random.Next(1, 11), random.Next(1, 11));
                }
                while (target.GetTileState(point) != TileState.Unknown);

                MissileLaunch(radarBoard.GridToScreen(Vector2.One * 5), oceanBoard.GridToScreen(point.ToVector2()));
            }
        }

        private void Shoot()
        {
            if (mouseState.LeftButton.Equals(ButtonState.Pressed) && oldMouseState.LeftButton.Equals(ButtonState.Released) 
                && radarBoard.IsMouseOver(MousePosition()))
            {
                Point point = radarBoard.ScreenToGrid(MousePosition()).ToPoint();
                if (radarBoard.GetTileState(point) == TileState.Unknown)
                {
                    MissileLaunch(oceanBoard.GridToScreen(Vector2.One * 5), radarBoard.GridToScreen(point.ToVector2()));
                }
            }
        }

        private void MissileLaunch(Vector2 start, Vector2 target)
        {
            missileVisible = true;
            missile.Position = start + Vector2.One * (Board.TileSize / 2);
            missile.Target = target + Vector2.One * (Board.TileSize / 2);
            missile.State = Missile.MissileState.Airborne;
        }

        private void MissileFly(float dt)
        {
            Vector2 dir = missile.Direction();
            dir.Normalize();
            missile.Position += missile.Velocity * dir * dt;

            Rectangle target = new Rectangle(missile.Target.ToPoint(), new Point(1));
            target.Inflate(2, 2);

            if (target.Contains(missile.Position))
            {
                missile.State = Missile.MissileState.Impact;
            }
        }

        private void MissileImpact(ref Board board, GameState next)
        {
            missileVisible = false;
            board.SetTileState(board.ScreenToGrid(missile.Target).ToPoint(), board.GetMissileHitResult(board.ScreenToGrid(missile.Target).ToPoint()));
            SwitchGameState(next);
            missile.State = Missile.MissileState.Launch;
        }

        //private bool IsShipInsideGrid(Ship ship)
        //{
        //    if (ship.Position.X < 1 || ship.Position.X > Board.Size)
        //        return false;
        //    if (ship.Position.Y < 1 || ship.Position.Y > Board.Size)
        //        return false;

        //    if (ship.Rotation % 2 == 0)
        //    {
        //        if (ship.Position.Y + ship.GetLength() > Board.Size + 1)
        //            return false;
        //    }
        //    else
        //    {
        //        if (ship.Position.X + ship.GetLength() > Board.Size + 1)
        //            return false;
        //    }

        //    return true;
        //}

        //private bool MouseOverShip()
        //{
        //    foreach (Ship ship in oceanBoard.Ships)
        //    {
        //        Rectangle shipRect = shipSheet[ship.GetSheetIndex()];
        //        shipRect.Location = oceanBoard.GridToScreen(ship.Position.ToVector2()).ToPoint();
        //        // todo: check without mouse position
        //        if (shipRect.Contains(oceanBoard.GridToScreen(radarBoard.ScreenToGrid(MousePosition()))))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        //private void MouseMoveShip()
        //{
        //    if (mouseState.RightButton.Equals(ButtonState.Pressed) /*&& oldMouseState.RightButton.Equals(ButtonState.Released)*/
        //        && oceanBoard.IsMouseOver(MousePosition()) && selectedIndex != -1)
        //    {
        //        Ship temp = oceanBoard.Ships[selectedIndex];
        //        temp.Position = oceanBoard.ScreenToGrid(oceanBoard.SnapToGrid(MousePosition())).ToPoint();
        //        if (IsShipInsideGrid(temp) && !DoesShipOverlapAny(oceanBoard.Ships, temp))
        //        {
        //            oceanBoard.Ships[selectedIndex] = temp;

        //            shipInside = IsShipInsideGrid(temp).ToString();
        //            shipOverlap = DoesShipOverlapAny(oceanBoard.Ships, temp).ToString();
        //        }
        //    }
        //}

        //private void SelectShip()
        //{
        //    if (mouseState.RightButton.Equals(ButtonState.Pressed) && oldMouseState.RightButton.Equals(ButtonState.Released))
        //    {
        //        if (oceanBoard.IsMouseOver(MousePosition()))
        //        {
        //            for (int i = 0; i < oceanBoard.Ships.Length; i++)
        //            {
        //                Ship ship = oceanBoard.Ships[i];
        //                Rectangle shipRect = shipSheet[ship.GetSheetIndex()];
        //                shipRect.Location = oceanBoard.GridToScreen(ship.Position.ToVector2()).ToPoint();
        //                // todo: check without mouse position
        //                if (shipRect.Contains(oceanBoard.GridToScreen(oceanBoard.ScreenToGrid(MousePosition()))))
        //                {
        //                    // todo: this is not a ref, so the ship in the list doesnt change position if we move it
        //                    ship1 = ship;
        //                    selectedIndex = i;
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            selectedIndex = -1;
        //        }
        //    }
        //}

        //private void ShipSelectTypeAndRotation()
        //{
        //    if (KeyJustPressed(Keys.Left))
        //    {
        //        ship1.Type = (ShipType)MathHelper.Clamp((int)ship1.Type - 1, 0, 4);
        //    }
        //    if (KeyJustPressed(Keys.Right))
        //    {
        //        ship1.Type = (ShipType)MathHelper.Clamp((int)ship1.Type + 1, 0, 4);
        //    }

        //    if (KeyJustPressed(Keys.Up))
        //    {
        //        ship1.Rotation = MathHelper.Clamp(ship1.Rotation - 1, 0, 3);
        //    }
        //    if (KeyJustPressed(Keys.Down))
        //    {
        //        ship1.Rotation = MathHelper.Clamp(ship1.Rotation + 1, 0, 3);
        //    }
        //}

        //private void MouseMoveTarget()
        //{
        //    if (mouseState.LeftButton.Equals(ButtonState.Pressed) && oldMouseState.LeftButton.Equals(ButtonState.Released) 
        //        && radarBoard.IsMouseOver(MousePosition()))
        //    {
        //        tokenPos = radarBoard.SnapToGrid(MousePosition());
        //    }
        //}

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(panel, GraphicsDevice.Viewport.Bounds, Color.White);

            _spriteBatch.Draw(radarGrid, radarBoard.LocalPosition, Color.White);
            _spriteBatch.Draw(oceanGrid, oceanBoard.LocalPosition, Color.White);

            _spriteBatch.Draw(radarAnimation, radarBoard.GridToScreen(Vector2.One), radarSheet["radar_anim" + ((int)(radarAnimationTime / radarAnimationDuration * 360)).ToString("000")], Color.White);
            _spriteBatch.Draw(radarAnimGrid, radarBoard.GridToScreen(Vector2.Zero), Color.White);

            //_spriteBatch.Draw(tokens, tokenPos, tokenSheet.Sprites[token], Color.White);
            _spriteBatch.Draw(shipGrid, oceanBoard.GridToScreen(ship1.Position.ToVector2()), shipSheet[ship1.GetSheetIndex()], Color.White);

            foreach (Ship ship in oceanBoard.Ships)
            {
                _spriteBatch.Draw(shipGrid, oceanBoard.GridToScreen(ship.Position.ToVector2()), shipSheet[ship.GetSheetIndex()], Color.White);
            }

            if (radarShipsVisible)
                foreach (Ship ship in radarBoard.Ships)
                {
                    _spriteBatch.Draw(shipGrid, radarBoard.GridToScreen(ship.Position.ToVector2()), shipSheet[ship.GetSheetIndex()], Color.White);
                }

            foreach (var tuple in oceanBoard.Tiles)
            {
                if (tuple.Value != TileState.Unknown)
                {
                    _spriteBatch.Draw(tokens, oceanBoard.GridToScreen(tuple.Key.ToVector2()), tokenSheet[tuple.Value == TileState.Hit ? 3 : 2], Color.White);
                }
            }

            foreach (var tuple in radarBoard.Tiles)
            {
                if (tuple.Value != TileState.Unknown)
                {
                    _spriteBatch.Draw(tokens, radarBoard.GridToScreen(tuple.Key.ToVector2()), tokenSheet[tuple.Value == TileState.Hit ? 1 : 0], Color.White);
                }
            }

            if (blipVisible)
                _spriteBatch.Draw(blipAnimation, blipLocation, blipSheet["Blip_" + ((int)(blipAnimationTime / blipAnimationDuration * blipSheet.Count)).ToString("000")], Color.White, 0f, blipSheet["Blip_000"].Size.ToVector2() / 2, 1f, SpriteEffects.None, 0f);

            if (missileVisible)
                _spriteBatch.Draw(missileTexture, missile.Position, missileSheet["Missile_E3"], Color.White, missile.Rotation(), missileSheet["Missile_E3"].Size.ToVector2() / 2, 2f, SpriteEffects.None, 0f);
            //_spriteBatch.DrawString(font24, shipInside, new(200, 400), Color.White);
            //_spriteBatch.DrawString(font24, shipOverlap, new(300, 400), Color.White);
            //_spriteBatch.DrawString(font24, anyOverlap, new(400, 400), Color.White);

            //_spriteBatch.DrawString(font18, $"AllHit: {radarBoard.AllShipsHit()}", new Vector2(300, 450), Color.White);
            //_spriteBatch.DrawString(font18, $"AllHit: {oceanBoard.AllShipsHit()}", new Vector2(500, 450), Color.White);
            //_spriteBatch.DrawString(font18, $"State: {gameState}", new Vector2(20, 450), Color.White);

            // todo: display with shadow/different color
            Vector2 size = font18.MeasureString(statusText);
            _spriteBatch.DrawString(font18, statusText, new Vector2(GraphicsDevice.Viewport.Width / 2 - size.X / 2, 400), Color.White);

            _spriteBatch.End();

            // draws components
            base.Draw(gameTime);
        }
    }
}
