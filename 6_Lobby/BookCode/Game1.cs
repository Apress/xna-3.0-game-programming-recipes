using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
    public enum GameState { SignIn, SearchSession, CreateSession, InSession}
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        SpriteFont myFont;
        List<string> log = new List<string>();
        KeyboardState lastKeybState = Keyboard.GetState();
        GamePadState lastPadState = GamePad.GetState(PlayerIndex.One);
        string command = "";

        GameState currentGameState = GameState.SignIn;
        NetworkSession networkSession;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            myFont = Content.Load<SpriteFont>("myFont");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState padState = GamePad.GetState(PlayerIndex.One);
            if (padState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keybState = Keyboard.GetState();

            if (this.IsActive)
            {
                switch (currentGameState)
                {
                    case GameState.SignIn:
                        {
                            if (Gamer.SignedInGamers.Count < 1)
                            {
                                Guide.ShowSignIn(1, false);
                                log.Add("Opened User SignIn Interface");
                            }
                            else
                            {
                                currentGameState = GameState.SearchSession;
                                log.Add(Gamer.SignedInGamers[0].Gamertag + " logged in - proceed to SearchSession");
                            }                            
                        }
                        break;
                    case GameState.SearchSession:
                        {
                            NetworkSessionProperties findProperties = new NetworkSessionProperties();
                            findProperties[0] = 3;
                            findProperties[1] = 4096;

                            AvailableNetworkSessionCollection activeSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 4, findProperties);                            
                            if (activeSessions.Count == 0)
                            {
                                currentGameState = GameState.CreateSession;
                                log.Add("No active sessions found - proceed to CreateSession");
                            }
                            else
                            {
                                AvailableNetworkSession sessionToJoin = activeSessions[0];
                                networkSession = NetworkSession.Join(sessionToJoin);
                                
                                string myString = "Joined session hosted by " + sessionToJoin.HostGamertag;
                                myString += " with " + sessionToJoin.CurrentGamerCount.ToString() + " players";
                                myString += " and " + sessionToJoin.OpenPublicGamerSlots.ToString() + " open player slots.";
                                log.Add(myString);

                                HookSessionEvents();
                                command = "[Press X to signal you're ready]";
                                currentGameState = GameState.InSession;
                            }
                        }
                        break;
                    case GameState.CreateSession:
                        {
                            NetworkSessionProperties createProperties = new NetworkSessionProperties();
                            createProperties[0] = 3;
                            createProperties[1] = 4096;

                            networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 4, 16, 0, createProperties);
                            networkSession.AllowHostMigration = true;
                            networkSession.AllowJoinInProgress = false;
                            log.Add("New session created");

                            HookSessionEvents();

                            command = "[Press X to signal you're ready]";
                            currentGameState = GameState.InSession;
                        }
                        break;
                    case GameState.InSession:
                        {
                            switch (networkSession.SessionState)
                            {
                                case NetworkSessionState.Lobby:
                                    {
                                        if ((keybState != lastKeybState) || (padState != lastPadState))
                                        {
                                            if (keybState.IsKeyDown(Keys.X) || (padState.IsButtonDown(Buttons.X)))
                                            {
                                                LocalNetworkGamer localGamer = networkSession.LocalGamers[0];
                                                localGamer.IsReady = !localGamer.IsReady;
                                            }
                                        }

                                        if (networkSession.IsHost)
                                        {
                                            if (networkSession.AllGamers.Count > 1)
                                            {
                                                if (networkSession.IsEveryoneReady)
                                                {
                                                    networkSession.StartGame();
                                                    log.Add("All players ready -- start the game!");
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case NetworkSessionState.Playing:
                                    {
                                        if (networkSession.IsHost)
                                        {
                                            if ((keybState != lastKeybState) || (padState != lastPadState))
                                            {
                                                if (keybState.IsKeyDown(Keys.Y) || (padState.IsButtonDown(Buttons.Y)))
                                                {
                                                    networkSession.EndGame();
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }

                            networkSession.Update();                            
                        }
                        break;
                }
            }

            lastKeybState = keybState;

            base.Update(gameTime);
        }

        void HookSessionEvents()
        {
            log.Add("Listening for session events");
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.GamerLeft += GamerLeftEventHandler;
            networkSession.HostChanged += HostChangedEventHandler;

            networkSession.GameStarted += GameStartedEventHandler;
            networkSession.GameEnded += GameEndedEventHandler;
        }

        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            log.Add(e.Gamer.Gamertag + " joined the current session");
        }

        void GamerLeftEventHandler(object sender, GamerLeftEventArgs e)
        {
            log.Add(e.Gamer.Gamertag + " left the current session");
        }

        void HostChangedEventHandler(object sender, HostChangedEventArgs e)
        {
            log.Add("Host migration detected");

            NetworkSession eventRaisingSession = (NetworkSession)sender;
            if (eventRaisingSession.IsHost)
                log.Add("This machine has become the new Host!");
        }

        void GameStartedEventHandler(object sender, GameStartedEventArgs e)
        {
            log.Add("Game started!");
            if (networkSession.IsHost)
                command = "[You're the host; press Y to end the game and return to the lobby]";
            else
                command = "";
        }

        void GameEndedEventHandler(object sender, GameEndedEventArgs e)
        {
            log.Add("Game ended");            
            command = "[Press R to signal you're ready]";
        }
        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            //Print each entry of the log to the screen
            spriteBatch.Begin();
            int i = 0;
            foreach (string entry in log)
                spriteBatch.DrawString(myFont, entry, new Vector2(20, 20 * ++i), Color.White);

            if (networkSession != null)
                foreach (NetworkGamer gamer in networkSession.AllGamers)
                    spriteBatch.DrawString(myFont, gamer.Gamertag + " is ready: " + gamer.IsReady.ToString(), new Vector2(20, 20 * ++i), Color.Red);

            spriteBatch.DrawString(myFont, command, new Vector2(400, 20), Color.Yellow);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
