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

        QTNode rootNode;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(graphics.GraphicsDevice.Viewport, new Vector3(20, 10, 15), 0, 0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            cCross = new CoordCross(device);

            Texture2D grassTexture = Content.Load<Texture2D>("grass");

            Texture2D heightMap = Content.Load<Texture2D>("heightmap");
            int width = heightMap.Width;
            int height = heightMap.Height;

            float[,] heightData = TerrainUtils.LoadHeightData(heightMap);
            VertexPositionNormalTexture[] vertices = TerrainUtils.CreateTerrainVertices(heightData);
            int[] indices = TerrainUtils.CreateTerrainIndices(width, height);
            vertices = TerrainUtils.GenerateNormalsForTriangleStrip(vertices, indices);
            VertexPositionNormalTexture[,] vertexArray = Reshape1Dto2D<VertexPositionNormalTexture>(vertices, width, height);

            rootNode = new QTNode(vertexArray, device, grassTexture, 64);
        }

        private T[,] Reshape1Dto2D<T>(T[] vertices, int width, int height)
        {
            T[,] vertexArray = new T[width, height];
            int i = 0;
            for (int h = 0; h < height; h++)
                for (int w = 0; w < width; w++)
                    vertexArray[w, h] = vertices[i++];

            return vertexArray;
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

            //draw coordcross
            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            BoundingFrustum cameraFrustrum = new BoundingFrustum(fpsCam.ViewMatrix * fpsCam.ProjectionMatrix);
            QTNode.NodesRendered = 0;
            rootNode.Draw(Matrix.CreateTranslation(-250, -20, 250), fpsCam.ViewMatrix, fpsCam.ProjectionMatrix, cameraFrustrum);
            Window.Title = string.Format("{0} nodes rendered", QTNode.NodesRendered);

            base.Draw(gameTime);
        }
    }
}