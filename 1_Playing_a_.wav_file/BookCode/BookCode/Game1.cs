using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        SoundEffect myEffect;
        SoundEffectInstance myEffectInstance;

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
            spriteBatch = new SpriteBatch(device);
            spriteFont = Content.Load<SpriteFont>("Arial");
            myEffect = Content.Load<SoundEffect>("effect");
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

            if (keyState.IsKeyDown(Keys.Space) || (gamePadState.Buttons.B == ButtonState.Pressed))
            {
                if (myEffectInstance == null)
                {
                    SoundEffect.MasterVolume = 1.0f;
                    myEffect.Play(1,0, 0, true);
                }
            }

            if (myEffectInstance != null)
            {
                myEffectInstance.Volume *= 0.9f;
                if (myEffectInstance.State == SoundState.Stopped)
                {
                    myEffectInstance.Dispose();
                    myEffectInstance = null;
                }
            }

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, "Press Space to play the sound", new Vector2(20, 20), Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
