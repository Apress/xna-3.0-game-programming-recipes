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
            myVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
            
            VertexPositionTexture[] vertices = new VertexPositionTexture[5];
            int i = 0;

            vertices[i++] = new VertexPositionTexture(new Vector3(-5.0f, -3, -1), new Vector2(-0.5f, 1.5f));
            vertices[i++] = new VertexPositionTexture(new Vector3(-2.5f, 5, -1), new Vector2(0.5f, -1.5f));
            vertices[i++] = new VertexPositionTexture(new Vector3(0, -3, -1), new Vector2(1.5f, 1.5f));
            vertices[i++] = new VertexPositionTexture(new Vector3(2.5f, 5, -1), new Vector2(0.5f, -1.5f));
            vertices[i++] = new VertexPositionTexture(new Vector3(5.0f, -3, -1), new Vector2(-0.5f, 1.5f));

            vertBuffer = new VertexBuffer(device, VertexPositionTexture.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertBuffer.SetData(vertices, 0, vertices.Length);
        }

        private void InitIndices()
        {
            int[] indices = new int[6];
            int i = 0;
            indices[i++] = 0;
            indices[i++] = 1;
            indices[i++] = 2;

            indices[i++] = 2;
            indices[i++] = 3;
            indices[i++] = 4;

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
            device.RenderState.CullMode = CullMode.None;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.Texture = myTexture;
            basicEffect.TextureEnabled = true;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.Vertices[0].SetSource(vertBuffer, 0, VertexPositionTexture.SizeInBytes);
                device.Indices = indexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 5, 0, 2);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
