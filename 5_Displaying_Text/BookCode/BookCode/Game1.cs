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
using System.Text;

namespace BookCode
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        SpriteFont myFont;
        StringBuilder stringBuilder = new StringBuilder();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            myFont = Content.Load<SpriteFont>("ourFont");
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

            spriteBatch.Begin();
            string myString = "Elapsed seconds: " + gameTime.TotalGameTime.Seconds.ToString();
            spriteBatch.DrawString(myFont, myString, new Vector2(50, 20), Color.Tomato);

            Vector2 stringSize = myFont.MeasureString(myString);            
            string secondString = "Length of previous string: " + stringSize;            
            spriteBatch.DrawString(myFont, secondString, new Vector2(50, 100), Color.White, 0, new Vector2(0,0), 0.5f, SpriteEffects.None, 0);

            stringBuilder.Length = 0;
            stringBuilder.Append("StringBuilder example: ");
            for (int i = 0; i < 10; i++)
                stringBuilder.Append(i);
            spriteBatch.DrawString(myFont, stringBuilder, new Vector2(50, 180), Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
