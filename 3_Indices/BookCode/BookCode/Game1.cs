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

        private int[] indices;
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
            InitIndices();
            cCross = new CoordCross(device);
        }

        private void InitVertices()
        {            
            vertices = new VertexPositionColor[9];

            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Red);
            vertices[1] = new VertexPositionColor(new Vector3(1, 0, 0), Color.Green);
            vertices[2] = new VertexPositionColor(new Vector3(2, 0, 1), Color.Blue);

            vertices[3] = new VertexPositionColor(new Vector3(0, 1, -1), Color.Orange);
            vertices[4] = new VertexPositionColor(new Vector3(1, 1, 0), Color.Olive);
            vertices[5] = new VertexPositionColor(new Vector3(2, 1, 0), Color.Magenta);

            vertices[6] = new VertexPositionColor(new Vector3(0, 2, 0), Color.Yellow);
            vertices[7] = new VertexPositionColor(new Vector3(1, 2, 1), Color.Tomato);
            vertices[8] = new VertexPositionColor(new Vector3(2, 2, -1), Color.Plum);

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
        }

        private void InitIndices()
        {
            indices = new int[24];

            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 1;

            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 4;

            indices[6] = 1;
            indices[7] = 4;
            indices[8] = 5;

            indices[9] = 1;
            indices[10] = 5;
            indices[11] = 2;

            indices[12] = 3;
            indices[13] = 6;
            indices[14] = 7;

            indices[15] = 3;
            indices[16] = 7;
            indices[17] = 4;

            indices[18] = 4;
            indices[19] = 7;
            indices[20] = 5;

            indices[21] = 5;
            indices[22] = 7;
            indices[23] = 8;
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
            basicEffect.World = Matrix.CreateScale(2.0f);
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.VertexColorEnabled = true;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, 9, indices, 0, 8);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
