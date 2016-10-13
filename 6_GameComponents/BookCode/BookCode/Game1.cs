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
        QuatCam quatCam;
        BillboardGC billboardGC;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            billboardGC = new BillboardGC(this);
            Components.Add(billboardGC);
        }

        protected override void Initialize()
        {
            quatCam = new QuatCam(GraphicsDevice.Viewport);            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;            
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

            quatCam.Update(mouseState, keyState, gamePadState);

            billboardGC.camForward = quatCam.Forward;
            billboardGC.camPosition = quatCam.Position;
            billboardGC.viewMatrix = quatCam.ViewMatrix;
            billboardGC.projectionMatrix = quatCam.ProjectionMatrix;

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);
            base.Draw(gameTime);            
        }
    }
}
