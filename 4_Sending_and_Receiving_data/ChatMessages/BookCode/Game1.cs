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
        Dictionary<string, string> gamerTimes = new Dictionary<string, string>();
        Dictionary<string, string> lastChatMessage = new Dictionary<string, string>();

        GameState currentGameState = GameState.SignIn;
        NetworkSession networkSession;
        PacketWriter writer = new PacketWriter();
        PacketReader reader = new PacketReader();

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

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
                            AvailableNetworkSessionCollection activeSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 4, null);                            
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
                                currentGameState = GameState.InSession;
                            }
                        }
                        break;
                    case GameState.CreateSession:
                        {
                            networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 4, 16);
                            networkSession.AllowHostMigration = true;
                            networkSession.AllowJoinInProgress = false;
                            log.Add("New session created");

                            HookSessionEvents();
                            currentGameState = GameState.InSession;                            
                        }
                        break;
                    case GameState.InSession:
                        {
                            //send data to all other players in session
                            writer.Write("D");
                            writer.Write(gameTime.TotalGameTime.Minutes);
                            writer.Write(gameTime.TotalGameTime.Seconds);

                            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];
                            localGamer.SendData(writer, SendDataOptions.ReliableInOrder);

                            writer.Write("C");
                            writer.Write("This is a chat message from " + localGamer.Gamertag);
                            localGamer.SendData(writer, SendDataOptions.Chat|SendDataOptions.ReliableInOrder);

                            //receive data from all other players in session                            
                            while (localGamer.IsDataAvailable)
                            {
                                NetworkGamer sender;
                                localGamer.ReceiveData(reader, out sender);

                                string messageType = reader.ReadString();
                                if (messageType == "D")
                                {
                                    string gamerTime = "";
                                    gamerTime += sender.Gamertag + ": ";
                                    gamerTime += reader.ReadInt32() + "m ";
                                    gamerTime += reader.ReadInt32() + "s";

                                    gamerTimes[sender.Gamertag] = gamerTime;
                                }
                                else if (messageType == "C")
                                {
                                    lastChatMessage[sender.Gamertag] = reader.ReadString();
                                }                                
                            }                            
                            
                            networkSession.Update();
                        }
                        break;
                }
            }

            base.Update(gameTime);
        }

        void HookSessionEvents()
        {
            log.Add("Listening for session events");
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.GamerLeft += GamerLeftEventHandler;
            networkSession.HostChanged += HostChangedEventHandler;            
        }

        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            log.Add(e.Gamer.Gamertag + " joined the current session");
        }

        void GamerLeftEventHandler(object sender, GamerLeftEventArgs e)
        {
            log.Add(e.Gamer.Gamertag + " left the current session");
            gamerTimes.Remove(e.Gamer.Gamertag);
            lastChatMessage.Remove(e.Gamer.Gamertag);
        }

        void HostChangedEventHandler(object sender, HostChangedEventArgs e)
        {
            log.Add("Host migration detected");

            NetworkSession eventRaisingSession = (NetworkSession)sender;
            if (eventRaisingSession.IsHost)
                log.Add("This machine has become the new Host!");
        }
        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            //Print each entry of the log to the screen
            spriteBatch.Begin();
            int i = 0;

            foreach (string entry in log)
                spriteBatch.DrawString(myFont, entry, new Vector2(20, 20 * ++i), Color.White);
            
            foreach (string gamerTime in gamerTimes.Values)
                spriteBatch.DrawString(myFont, gamerTime, new Vector2(20, 20 * ++i), Color.Red);

            foreach (string chatMessage in lastChatMessage.Values)
                spriteBatch.DrawString(myFont, chatMessage, new Vector2(20, 20 * ++i), Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
