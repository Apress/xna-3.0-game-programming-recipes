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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);            
            Content.RootDirectory = "Content";            
        }
        
        protected override void Initialize()
        {
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.ApplyChanges();
            IsFixedTimeStep = false;
            
            base.Initialize();            
        }

        protected override void LoadContent()
        {               
        }
     
        protected override void UnloadContent()
        {
        }
    
        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            float fps = (1000.0f/(float)gameTime.ElapsedRealTime.TotalMilliseconds);
            Window.Title = "FPS: " + fps.ToString();

            base.Update(gameTime);
        }
            
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);
            base.Draw(gameTime);
        }
    }
}