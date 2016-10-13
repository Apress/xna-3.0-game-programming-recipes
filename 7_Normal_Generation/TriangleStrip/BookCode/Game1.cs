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

        VertexPositionNormalTexture[] vertices;
        VertexDeclaration myVertexDeclaration;
        int[] indices;

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
            myTexture = Content.Load<Texture2D>("XNAtexture");

            InitVertices();
            InitIndices();
            vertices = GenerateNormalsForTriangleStrip(vertices, indices);
        }

        private void InitVertices()
        {
            myVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);

            List<VertexPositionNormalTexture> verticesList = new List<VertexPositionNormalTexture>();

            int detail = 20;

            float radius = 2;
            float height = 8;

            for (int i = 0; i < detail+1; i++)
			{
                float angle = MathHelper.Pi*2.0f/(float)detail*(float)i;
                Vector3 baseVector = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(angle));
                
                Vector3 posLow = baseVector*radius;
                posLow.Y = -height / 2.0f;

                Vector3 posHigh = posLow;
                posHigh.Y += height;

                Vector2 texCoordLow = new Vector2(angle / (MathHelper.Pi * 2.0f), 1);
                Vector2 texCoordHigh = new Vector2(angle / (MathHelper.Pi * 2.0f), 0);

                verticesList.Add(new VertexPositionNormalTexture(posLow, Vector3.Zero, texCoordLow));
                verticesList.Add(new VertexPositionNormalTexture(posHigh, Vector3.Zero, texCoordHigh));                
			}

            vertices = verticesList.ToArray();
        }
        
        private void InitIndices()
        {
            indices = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                indices[i] = i;
        }

        private VertexPositionNormalTexture[] GenerateNormalsForTriangleStrip(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            bool swappedWinding = false;
            for (int i = 2; i < indices.Length; i++)
            {
                Vector3 firstVec = vertices[indices[i - 1]].Position - vertices[indices[i]].Position;
                Vector3 secondVec = vertices[indices[i - 2]].Position - vertices[indices[i]].Position;
                Vector3 normal = Vector3.Cross(firstVec, secondVec);
                normal.Normalize();

                if (swappedWinding)
                    normal *= -1;

                if (!float.IsNaN(normal.X))
                {
                    vertices[indices[i]].Normal += normal;
                    vertices[indices[i - 1]].Normal += normal;
                    vertices[indices[i - 2]].Normal += normal;
                }

                swappedWinding = !swappedWinding;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
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
            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;

            basicEffect.Texture = myTexture;
            basicEffect.TextureEnabled = true;

            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            basicEffect.EnableDefaultLighting();
            basicEffect.DirectionalLight0.Direction = Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationY(time));
            basicEffect.DirectionalLight1.Enabled = false;
            basicEffect.DirectionalLight2.Enabled = false;
            basicEffect.SpecularColor = new Vector3(0, 0, 0);


            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, indices, 0, indices.Length - 2);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
