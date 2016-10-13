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
        QuakeCamera fpsCam;
        CoordCross cCross;

        Terrain terrain;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(graphics.GraphicsDevice.Viewport, new Vector3(1,15,-1), 0, 0);
            base.Initialize();
        }

        protected override void LoadContent()
        {   
            device = graphics.GraphicsDevice;
            cCross = new CoordCross(device);

            terrain = new Terrain(device, Content);
        }        
     
        protected override void UnloadContent()
        {
           Content.Unload();
        }
    
        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();
            
            fpsCam.Update(mouseState, keyState, gamePadState);

            float treshold = 3.0f;
            float terrainHeight = terrain.GetExactHeightAt(fpsCam.Position.X, -fpsCam.Position.Z);            
            Vector3 newPos = fpsCam.Position;
            newPos.Y = terrainHeight + treshold;
            fpsCam.Position = newPos;
            
            base.Update(gameTime);
        }
            
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            //draw coordcross
            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            terrain.Draw(Matrix.Identity, fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);
            
            base.Draw(gameTime);
        }
    }
}