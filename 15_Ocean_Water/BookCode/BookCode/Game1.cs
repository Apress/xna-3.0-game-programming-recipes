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

        Effect effect;
        int waterWidth = 128;
        int waterHeight = 128;
        VertexBuffer waterVertexBuffer;
        IndexBuffer waterIndexBuffer;
        VertexDeclaration myVertexDeclaration;
        Texture waterBumps;

        Model skyboxModel;
        TextureCube skyboxTexture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(110, 1.1f, -110), MathHelper.Pi * 3.0f / 4.0f, 0);
            
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            effect = Content.Load<Effect>("oceanwater");
            skyboxModel = Content.Load<Model>("skybox");
            skyboxTexture = Content.Load<TextureCube>("skybox02");
            waterBumps = Content.Load<Texture2D>("waterbumps");

            VertexPositionTexture[] waterVertices = CreateWaterVertices();
            int[] waterIndices = CreateWaterIndices();
            CreateBuffers(waterVertices, waterIndices);
        }

        private VertexPositionTexture[] CreateWaterVertices()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[waterWidth * waterHeight];

            int i = 0;
            for (int z = 0; z < waterHeight; z++)
            {
                for (int x = 0; x < waterWidth; x++)
                {
                    Vector3 position = new Vector3(x, 0, -z);
                    Vector2 texCoord = new Vector2((float)x / 30.0f, (float)z / 30.0f);

                    waterVertices[i++] = new VertexPositionTexture(position, texCoord);
                }
            }

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

            return waterVertices;
        }

        private int[] CreateWaterIndices()
        {
            int[] waterIndices = new int[(waterWidth) * 2 * (waterHeight - 1)];

            int i = 0;
            int z = 0;
            while (z < waterHeight - 1)
            {
                for (int x = 0; x < waterWidth; x++)
                {
                    waterIndices[i++] = x + z * waterWidth;
                    waterIndices[i++] = x + (z + 1) * waterWidth;
                }
                z++;

                if (z < waterHeight - 1)
                {
                    for (int x = waterWidth - 1; x >= 0; x--)
                    {
                        waterIndices[i++] = x + (z + 1) * waterWidth;
                        waterIndices[i++] = x + z * waterWidth;
                    }
                }
                z++;
            }

            return waterIndices;
        }

        private void CreateBuffers(VertexPositionTexture[] vertices, int[] indices)
        {
            waterVertexBuffer = new VertexBuffer(device, VertexPositionTexture.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(vertices);

            waterIndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            waterIndexBuffer.SetData(indices);
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

            //draw the skybox
            device.RenderState.DepthBufferWriteEnable = false;
            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (Effect skyboxEffect in mesh.Effects)
                {
                    skyboxEffect.Parameters["Texture0"].SetValue(skyboxTexture);
                    skyboxEffect.Parameters["matView"].SetValue(fpsCam.ViewMatrix);
                    skyboxEffect.Parameters["matProj"].SetValue(fpsCam.ProjectionMatrix);
                }
                mesh.Draw();
            }
            device.RenderState.DepthBufferWriteEnable = true;
            device.RenderState.DepthBufferEnable = true; //was set to false inside the effect file            

            //draw water                        
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            Vector4 waveFreqs = new Vector4(1, 2, 0.5f, 1.5f);
            Vector4 waveHeights = new Vector4(0.3f, 0.4f, 0.2f, 0.3f);
            //waveHeights = new Vector4(0, 0, 0, 0);
            Vector4 waveLengths = new Vector4(10, 5, 15, 7);

            Vector2[] waveDirs = new Vector2[4];
            waveDirs[0] = new Vector2(-1, 0);
            waveDirs[1] = new Vector2(-1, 0.5f);
            waveDirs[2] = new Vector2(-1, 0.7f);
            waveDirs[3] = new Vector2(-1, -0.5f);

            for (int i = 0; i < 4; i++)
                waveDirs[i].Normalize();

            effect.CurrentTechnique = effect.Techniques["OceanWater"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            effect.Parameters["xBumpMap"].SetValue(waterBumps);
            effect.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            effect.Parameters["xBumpStrength"].SetValue(0.5f);

            effect.Parameters["xCubeMap"].SetValue(skyboxTexture);
            effect.Parameters["xTexStretch"].SetValue(4.0f);
            effect.Parameters["xCameraPos"].SetValue(fpsCam.Position);
            effect.Parameters["xTime"].SetValue(time);

            effect.Parameters["xWaveSpeeds"].SetValue(waveFreqs);
            effect.Parameters["xWaveHeights"].SetValue(waveHeights);
            effect.Parameters["xWaveLengths"].SetValue(waveLengths);
            effect.Parameters["xWaveDir0"].SetValue(waveDirs[0]);
            effect.Parameters["xWaveDir1"].SetValue(waveDirs[1]);
            effect.Parameters["xWaveDir2"].SetValue(waveDirs[2]);
            effect.Parameters["xWaveDir3"].SetValue(waveDirs[3]);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(waterVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                device.Indices = waterIndexBuffer;
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, waterWidth * waterHeight, 0, waterWidth * 2 * (waterHeight - 1) - 2);

                pass.End();
            }
            effect.End();

            base.Draw(gameTime);
        }
    }
}
