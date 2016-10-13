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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Texture2D myTexture;

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

            myTexture = DefineTextureColors();
            myTexture.Save("savedtexture.jpg", ImageFileFormat.Jpg);
        }

        private Texture2D DefineTextureColors()
        {
            int textureWidth = 512;
            int textureHeight = 512;

            Color[] textureColors = new Color[textureWidth * textureHeight];
            int i = 0;
            for (int ver = 0; ver < textureHeight; ver++)
                for (int hor = 0; hor < textureWidth; hor++)
                {
                    float red = (float)hor / (float)textureWidth;
                    float green = 0;
                    float blue = (float)ver / (float)textureHeight;
                    float alpha = 1;

                    textureColors[i++] = new Color(new Vector4(red, green, blue, alpha));
                }

            Texture2D newTexture = new Texture2D(device, textureWidth, textureHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            newTexture.SetData<Color>(textureColors);

            return newTexture;
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
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            spriteBatch.Begin();
            spriteBatch.Draw(myTexture, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
