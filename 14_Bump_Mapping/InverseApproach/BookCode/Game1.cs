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
    public struct VertPosTexNormTan
    {
        public Vector3 Position;
        public Vector2 TexCoords;
        public Vector3 Normal;
        public Vector3 Tangent;
        public VertPosTexNormTan(Vector3 Position, Vector2 TexCoords, Vector3 Normal, Vector3 Tangent)
        {
            this.Position = Position;
            this.TexCoords = TexCoords;
            this.Normal = Normal;
            this.Tangent = Tangent;
        }

        public static readonly VertexElement[] VertexElements =
            {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, sizeof(float)*3, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0, sizeof(float)*(3+2), VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
                new VertexElement(0, sizeof(float)*(3+2+3), VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Tangent, 0),
            };
        public static readonly int SizeInBytes = sizeof(float) * (3 + 2 + 3 + 3);
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        QuakeCamera fpsCam;
        VertPosTexNormTan[] vertices;
        VertexDeclaration myVertexDeclaration;
        int[] indices;        
        Texture2D myTexture;
        Texture2D myBumpMap;
        Effect effect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(0, 0, 12), 0, 0);            
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;                        
            myTexture = Content.Load<Texture2D>("wall");
            myBumpMap = Content.Load<Texture2D>("wall_normal_map");
            effect = Content.Load<Effect>("bumpmapping");

            InitVertices();
            InitIndices();
            vertices = GenerateNormalsForTriangleList(vertices, indices);
        }

        private void InitVertices()
        {
            List<VertPosTexNormTan> verticesList = new List<VertPosTexNormTan>();

            int detail = 20;

            float radius = 2;
            float height = 8;

            for (int i = 0; i < detail + 1; i++)
            {
                float angle = MathHelper.Pi * 2.0f / (float)detail * (float)i;
                Vector3 baseVector = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(angle));

                Vector3 posLow = baseVector * radius;
                posLow.Y = -height / 2.0f;

                Vector3 posHigh = posLow;
                posHigh.Y += height;

                Vector2 texCoordLow = new Vector2(angle / (MathHelper.Pi * 2.0f), 1);
                Vector2 texCoordHigh = new Vector2(angle / (MathHelper.Pi * 2.0f), 0);

                verticesList.Add(new VertPosTexNormTan(posLow, texCoordLow, Vector3.Zero, new Vector3(0, 1, 0)));
                verticesList.Add(new VertPosTexNormTan(posHigh, texCoordHigh, Vector3.Zero, new Vector3(0, 1, 0)));
            }

            vertices = verticesList.ToArray();

            myVertexDeclaration = new VertexDeclaration(device, VertPosTexNormTan.VertexElements);
        }

        private int[] InitIndices()
        {
            int triangles = vertices.Length - 2;
            indices = new int[triangles * 3];

            int j = 0;
            for (int i = 0; i < vertices.Length / 2 - 1; i++)
            {
                indices[j++] = i * 2;
                indices[j++] = i * 2 + 1;
                indices[j++] = i * 2 + 2;

                indices[j++] = i * 2 + 1;
                indices[j++] = i * 2 + 3;
                indices[j++] = i * 2 + 2;
            }

            return indices;
        }

        private VertPosTexNormTan[] GenerateNormalsForTriangleList(VertPosTexNormTan[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                Vector3 firstVec = vertices[indices[i * 3 + 1]].Position - vertices[indices[i * 3]].Position;
                Vector3 secondVec = vertices[indices[i * 3 + 2]].Position - vertices[indices[i * 3]].Position;
                Vector3 normal = Vector3.Cross(secondVec, firstVec);
                normal.Normalize();

                if (!float.IsNaN(normal.X))
                {
                    vertices[indices[i * 3]].Normal += normal;
                    vertices[indices[i * 3 + 1]].Normal += normal;
                    vertices[indices[i * 3 + 2]].Normal += normal;
                }
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
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);                        

            //set effect parameters
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            Vector3 lightDirection = Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationY(time));

            effect.CurrentTechnique = effect.Techniques["BumpMapping"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            effect.Parameters["xTexture"].SetValue(myTexture);
            effect.Parameters["xBumpMap"].SetValue(myBumpMap);
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xTexStretch"].SetValue(4.0f);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserIndexedPrimitives<VertPosTexNormTan>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
                pass.End();
            }
            effect.End();

            base.Draw(gameTime);
        }
    }
}
