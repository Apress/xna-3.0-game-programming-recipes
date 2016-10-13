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
            myTexture = Content.Load<Texture2D>("CLIFFVEG");
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

            Rectangle grassRec = new Rectangle(240, 121, 40, 40);
            Rectangle leftRec = new Rectangle(40, 121, 80, 40);
            Rectangle topleftRec = new Rectangle(40, 0, 80, 80);
            Rectangle topRec = new Rectangle(240, 0, 40, 80);
            Rectangle toprightRec = new Rectangle(320, 0, 80, 80);
            Rectangle rightRec = new Rectangle(320, 121, 80, 40);
            Rectangle bottomrightRec = new Rectangle(320, 281, 80, 120);
            Rectangle bottomRec = new Rectangle(240, 281, 40, 120);
            Rectangle bottomleftRec = new Rectangle(40, 281, 80, 120);
            Rectangle centerRec = new Rectangle(240, 201, 80, 40);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);

            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    spriteBatch.Draw(myTexture, new Vector2(x * 40, y * 40), grassRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);

            spriteBatch.Draw(myTexture, new Vector2(40, 120), leftRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(40, 40), topleftRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(120, 40), topRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(160, 40), toprightRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(160, 120), rightRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(160, 160), bottomrightRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(120, 160), bottomRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(40, 160), bottomleftRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);
            spriteBatch.Draw(myTexture, new Vector2(120, 120), centerRec, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.5f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
