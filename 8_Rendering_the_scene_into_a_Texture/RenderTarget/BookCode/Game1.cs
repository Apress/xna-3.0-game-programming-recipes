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
        BasicEffect basicEffect;
        CoordCross cCross;
        QuakeCamera fpsCam;
        RenderTarget2D renderTarget;
        Matrix rendertargetProjectionMatrix;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);
            spriteBatch = new SpriteBatch(device);

            PresentationParameters pp = device.PresentationParameters;
            int width = pp.BackBufferWidth / 2;
            int height = pp.BackBufferHeight / 4;
            renderTarget = new RenderTarget2D(device, width, height, 1, device.DisplayMode.Format);
            rendertargetProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)width / (float)height, 0.5f, 100.0f);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            fpsCam.Update(mouseState, keyState, gamePadState);

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            device.SetRenderTarget(0, renderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            cCross.Draw(fpsCam.ViewMatrix, rendertargetProjectionMatrix);

            device.SetRenderTarget(0, null);
            Texture2D resolvedTexture = renderTarget.GetTexture();

            graphics.GraphicsDevice.Clear(Color.Tomato);
            spriteBatch.Begin();
            spriteBatch.Draw(resolvedTexture, new Vector2(100, 100), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
