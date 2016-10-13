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

        Texture2D myTexture;
        VertexDeclaration myVertexDeclaration;
        VertexBuffer vertBuffer;
        IndexBuffer indexBuffer;

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
            myTexture = Content.Load<Texture2D>("XNAtexture");
            InitVertices();
            InitIndices();
            cCross = new CoordCross(device);
        }

        private void InitVertices()
        {
            myVertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            
            VertexPositionColor[] vertices = new VertexPositionColor[8];

            //top forward left
            vertices[0] = new VertexPositionColor(new Vector3(-3, 3, -3), Color.Red);
            //top forward right
            vertices[1] = new VertexPositionColor(new Vector3(3, 3, -3), Color.Blue);
            //top back right
            vertices[2] = new VertexPositionColor(new Vector3(3, 3, 3), Color.Green);
            //top back left
            vertices[3] = new VertexPositionColor(new Vector3(-3, 3, 3), Color.White);

            //bottom foward left
            vertices[4] = new VertexPositionColor(new Vector3(-3, -3, -3), Color.White);
            //bottom forward right
            vertices[5] = new VertexPositionColor(new Vector3(3, -3, -3), Color.Green);
            //bottom back right
            vertices[6] = new VertexPositionColor(new Vector3(3, -3, 3), Color.White);
            //bottom back left
            vertices[7] = new VertexPositionColor(new Vector3(-3, -3, 3), Color.Red);            

            vertBuffer = new VertexBuffer(device, VertexPositionColor.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertBuffer.SetData(vertices, 0, vertices.Length);
        }

        private void InitIndices()
        {
            int[] indices = new int[36];
            int i = 0;
            //bottom
            indices[i++] = 5;
            indices[i++] = 4;
            indices[i++] = 6;

            indices[i++] = 7;
            indices[i++] = 6;
            indices[i++] = 4;

            //top
            indices[i++] = 0;
            indices[i++] = 1;
            indices[i++] = 2;

            indices[i++] = 2;
            indices[i++] = 3;
            indices[i++] = 0;

            //left
            indices[i++] = 0;
            indices[i++] = 3;
            indices[i++] = 7;

            indices[i++] = 7;
            indices[i++] = 4;
            indices[i++] = 0;

            //back
            indices[i++] = 3;
            indices[i++] = 2;
            indices[i++] = 6;

            indices[i++] = 6;
            indices[i++] = 7;
            indices[i++] = 3;

            //right
            indices[i++] = 2;
            indices[i++] = 1;
            indices[i++] = 5;

            indices[i++] = 5;
            indices[i++] = 6;
            indices[i++] = 2;

            //front
            indices[i++] = 0;
            indices[i++] = 4;
            indices[i++] = 5;

            indices[i++] = 5;
            indices[i++] = 1;
            indices[i++] = 0;

            
            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
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
            //device.RenderState.CullMode = CullMode.None;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.VertexColorEnabled = true;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.Vertices[0].SetSource(vertBuffer, 0, VertexPositionColor.SizeInBytes);
                device.Indices = indexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
