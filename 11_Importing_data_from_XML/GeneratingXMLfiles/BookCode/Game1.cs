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
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace XMLDataPLine
{
    public class MapData
    {
        public string mapName;
        public int numberOfCastles;
        public List<string> allies = new List<string>();
    }
}

namespace BookCode
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            XMLDataPLine.MapData myMap = new XMLDataPLine.MapData();
            myMap.mapName = "Battle In The Middle";
            myMap.numberOfCastles = 8;
            myMap.allies.Add("Humans");
            myMap.allies.Add("Elves");
            myMap.allies.Add("Dwarves");

            string fileName = "data.xml";
            XmlWriter writer = XmlWriter.Create(fileName);
            IntermediateSerializer.Serialize<XMLDataPLine.MapData>(writer, myMap, fileName);
            writer.Close();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = GraphicsDevice;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            base.Draw(gameTime);
        }
    }
}
