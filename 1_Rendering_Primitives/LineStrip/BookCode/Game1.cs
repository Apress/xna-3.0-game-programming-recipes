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
        QuakeCamera fpsCam;
        CoordCross cCross;

        VertexPositionColor[] vertices;
        VertexDeclaration myVertexDeclaration;

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
            InitVertices();
            cCross = new CoordCross(device);
        }

        private void InitVertices()
        {
            myVertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            vertices = new VertexPositionColor[12];

            vertices[0] = new VertexPositionColor(new Vector3(-5, 1, 1), Color.Red);
            vertices[1] = new VertexPositionColor(new Vector3(-5, 5, 1), Color.Green);
            vertices[2] = new VertexPositionColor(new Vector3(-3, 1, 1), Color.Blue);

            vertices[3] = new VertexPositionColor(new Vector3(-3, 5, 1), Color.Gray);
            vertices[4] = new VertexPositionColor(new Vector3(-1, 1, 1), Color.Purple);
            vertices[5] = new VertexPositionColor(new Vector3(-1, 5, 1), Color.Orange);

            vertices[6] = new VertexPositionColor(new Vector3(1, 1, 1), Color.BurlyWood);
            vertices[7] = new VertexPositionColor(new Vector3(1, 5, 1), Color.Gray);
            vertices[8] = new VertexPositionColor(new Vector3(3, 1, 1), Color.Green);

            vertices[9] = new VertexPositionColor(new Vector3(3, 5, 1), Color.Yellow);
            vertices[10] = new VertexPositionColor(new Vector3(5, 1, 1), Color.Blue);
            vertices[11] = new VertexPositionColor(new Vector3(5, 5, 1), Color.Red);
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
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            //draw triangles
            device.RenderState.CullMode = CullMode.None;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.VertexColorEnabled = true;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertices, 0, 11);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
