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
using System.IO;
using System.Xml.Serialization;

namespace BookCode
{
    [Serializable]
    public struct GameData
    {
        public int ActivePlayers;
        public float Time;
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont myFont;
        List<string> log = new List<string>();
        bool operationPending = false;

        GameData gameData;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));
        }

        protected override void Initialize()
        {
            gameData = new GameData();
            gameData.ActivePlayers = 2;
            gameData.Time = 0;

            log.Add("Hit S to save the current game data");
            log.Add("Hit L to load the current game data");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            myFont = Content.Load<SpriteFont>("myFont");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keyState = Keyboard.GetState();
            if (!Guide.IsVisible && !operationPending)
            {
                if (keyState.IsKeyDown(Keys.S) || (gamePadState.Buttons.B == ButtonState.Pressed))
                {
                    operationPending = true;
                    Guide.BeginShowStorageDeviceSelector(FindStorageDevice, "saveRequest");                    
                }
                if (keyState.IsKeyDown(Keys.L) || (gamePadState.Buttons.A == ButtonState.Pressed))
                {
                    operationPending = true;
                    Guide.BeginShowStorageDeviceSelector(FindStorageDevice, "loadRequest");                    
                }                
            }

            gameData.Time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        private void FindStorageDevice(IAsyncResult result)
        {
            StorageDevice storageDevice = Guide.EndShowStorageDeviceSelector(result);
            if (storageDevice != null)
            {
                if (result.AsyncState == "saveRequest")
                    SaveGame(storageDevice);
                else if (result.AsyncState == "loadRequest")
                    LoadGame(storageDevice);
            }
        }

        private void SaveGame(StorageDevice storageDevice)
        {
            StorageContainer container = storageDevice.OpenContainer("BookCodeWin");
            string fileName = Path.Combine(container.Path, "save0001.sav");

            FileStream saveFile = File.Open(fileName, FileMode.Create);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameData));

            xmlSerializer.Serialize(saveFile, gameData);
            saveFile.Close();
            container.Dispose();

            log.Add("Game data saved!");
            operationPending = false;
        }

        private void LoadGame(StorageDevice storageDevice)
        {
            StorageContainer container = storageDevice.OpenContainer("BookCodeWin");
            string fileName = Path.Combine(container.Path, "save0001.sav");
            if (File.Exists(fileName))
            {
                FileStream saveFile = File.Open(fileName, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameData));

                gameData = (GameData)xmlSerializer.Deserialize(saveFile);
                saveFile.Close();

                log.Add("Game data loaded!");
                operationPending = false;
            }
            container.Dispose();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);            

            spriteBatch.Begin();
            spriteBatch.DrawString(myFont, "savedata: "+gameData.Time.ToString(), new Vector2(30, 30), Color.Red);
            for (int i = 0; i < log.Count; i++)
                spriteBatch.DrawString(myFont, log[i], new Vector2(30, 30 * (i + 2)), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}