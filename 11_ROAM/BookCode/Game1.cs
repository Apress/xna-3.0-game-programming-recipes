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

        Texture2D grassTexture;
        float[,] heightData;
        VertexDeclaration myVertexDeclaration;
        VertexBuffer terrainVertexBuffer;
                
        List<Triangle> triangleList;
        List<int> indicesList;
        DynamicIndexBuffer dynTerrainIndexBuffer;

        Matrix orthoView;
        Matrix orthoProj;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(2, 30, 15), -MathHelper.PiOver2 * 0, -MathHelper.Pi / 4.0f);
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);
            grassTexture = Content.Load<Texture2D>("grass");            

            Texture2D heightMap = Content.Load<Texture2D>("heightmap");
            heightData = LoadHeightData(heightMap);                        

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
            VertexPositionNormalTexture[] terrainVertices = CreateTerrainVertices();
            int[] terrainIndices = CreateTerrainIndices();
            terrainVertices = GenerateNormalsForTriangleStrip(terrainVertices, terrainIndices);
            terrainVertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.SizeInBytes * terrainVertices.Length, BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(terrainVertices);

            int terrainSize = 1024;
            Triangle leftTriangle = new Triangle(null, new Vector2(0, 0), new Vector2(terrainSize, 0), new Vector2(0, terrainSize), heightData);
            Triangle rightTriangle = new Triangle(null, new Vector2(terrainSize, terrainSize), new Vector2(0, terrainSize), new Vector2(terrainSize, 0), heightData);
            leftTriangle.AddNeighs(null, null, rightTriangle);
            rightTriangle.AddNeighs(null, null, leftTriangle);

            triangleList = new List<Triangle>();
            triangleList.Add(leftTriangle);
            triangleList.Add(rightTriangle);            

            indicesList = new List<int>();
            foreach (Triangle t in triangleList)
                t.AddIndices(ref indicesList);

            dynTerrainIndexBuffer = new DynamicIndexBuffer(device, typeof(int), indicesList.Count, BufferUsage.WriteOnly);
            dynTerrainIndexBuffer.SetData(indicesList.ToArray(), 0, indicesList.Count, SetDataOptions.Discard);
            dynTerrainIndexBuffer.ContentLost += new EventHandler(dynIndexBuffer_ContentLost);

            orthoView = Matrix.CreateLookAt(new Vector3(terrainSize / 2, 100, -terrainSize / 2), new Vector3(terrainSize / 2, 0, -terrainSize / 2), Vector3.Forward);
            orthoProj = Matrix.CreateOrthographic(terrainSize, terrainSize, 1, 1000);
        }

        private void dynIndexBuffer_ContentLost(object sender, EventArgs e)
        {
            dynTerrainIndexBuffer.Dispose();
            dynTerrainIndexBuffer.SetData(indicesList.ToArray(), 0, indicesList.Count, SetDataOptions.Discard);
        }

        private float[,] LoadHeightData(Texture2D heightMap)
        {
            float minimumHeight = 255;
            float maximumHeight = 0;

            int width = heightMap.Width;
            int height = heightMap.Height;

            Color[] heightMapColors = new Color[width * height];
            heightMap.GetData<Color>(heightMapColors);

            float[,] heightData = new float[width + 1, height + 1];
            for (int x = 0; x < width - 1; x++)
                for (int y = 0; y < height - 1; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * width].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < width + 1; x++)
                heightData[x, height] = heightData[x, height - 1];

            for (int y = 0; y < height; y++)
                heightData[width, y] = heightData[width - 1, y];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;

            return heightData;
        }

        private float[,] LoadHeightDataAndCopyLastRowAndColumn(Texture2D heightMap)
        {
            float minimumHeight = 255;
            float maximumHeight = 0;

            int width = heightMap.Width;
            int height = heightMap.Height;

            Color[] heightMapColors = new Color[width * height];
            heightMap.GetData<Color>(heightMapColors);

            float[,] heightData = new float[width + 1, height + 1];
            for (int x = 0; x < width - 1; x++)
                for (int y = 0; y < height - 1; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * width].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < width + 1; x++)
                heightData[x, height] = heightData[x, height - 1];

            for (int y = 0; y < height; y++)
                heightData[width, y] = heightData[width - 1, y];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;

            return heightData;
        }

        private VertexPositionNormalTexture[] CreateTerrainVertices()
        {
            int width = heightData.GetLength(0);
            int height = heightData.GetLength(1);
            VertexPositionNormalTexture[] terrainVertices = new VertexPositionNormalTexture[width * height];

            int i = 0;
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 position = new Vector3(x, heightData[x, z], -z);
                    Vector3 normal = new Vector3(0, 0, 1);
                    Vector2 texCoord = new Vector2((float)x / 30.0f, (float)z / 30.0f);

                    terrainVertices[i++] = new VertexPositionNormalTexture(position, normal, texCoord);
                }
            }

            return terrainVertices;
        }

        private int[] CreateTerrainIndices()
        {
            int width = heightData.GetLength(0);
            int height = heightData.GetLength(1);

            int[] terrainIndices = new int[(width) * 2 * (height - 1)];

            int i = 0;
            int z = 0;
            while (z < height - 1)
            {
                for (int x = 0; x < width; x++)
                {
                    terrainIndices[i++] = x + z * width;
                    terrainIndices[i++] = x + (z + 1) * width;
                }
                z++;

                if (z < height - 1)
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        terrainIndices[i++] = x + (z + 1) * width;
                        terrainIndices[i++] = x + z * width;
                    }
                }
                z++;
            }

            return terrainIndices;
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
            dynTerrainIndexBuffer.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            fpsCam.Update(mouseState, keyState, gamePadState);

            UpdateTriangles();
            UpdateIndexBuffer();
            
            Window.Title = "Triangles drawn: " + triangleList.Count.ToString();            

            base.Update(gameTime);
        }

        private void UpdateTriangles()
        {
            List<Triangle> splitList = new List<Triangle>();
            List<Triangle> mergeList = new List<Triangle>();
            List<Triangle> remainderList = new List<Triangle>();
            List<Triangle> leftoverList = new List<Triangle>();
            List<Triangle> newTriangleList = new List<Triangle>(triangleList.Count);

            Matrix worldViewProjectionMatrix = Matrix.Identity * fpsCam.ViewMatrix * fpsCam.ProjectionMatrix;
            BoundingFrustum cameraFrustum = new BoundingFrustum(worldViewProjectionMatrix);            

            foreach (Triangle t in triangleList)
                t.CreateSplitList(ref splitList, ref remainderList, ref worldViewProjectionMatrix, ref cameraFrustum);

            foreach (Triangle t in splitList)
                t.ProcessSplitList(ref newTriangleList);

            foreach (Triangle t in remainderList)
                t.CreateMergeList(ref mergeList, ref leftoverList, ref worldViewProjectionMatrix, ref cameraFrustum);

            foreach (Triangle t in mergeList)
                t.ProcessMergeList(ref newTriangleList, ref worldViewProjectionMatrix, ref cameraFrustum);

            foreach (Triangle t in leftoverList)
                t.ProcessLeftovers(ref newTriangleList);

            triangleList = newTriangleList;
            triangleList.TrimExcess();
        }

        private void UpdateIndexBuffer()
        {
            indicesList.Clear();
            foreach (Triangle t in triangleList)
                t.AddIndices(ref indicesList);

            if (dynTerrainIndexBuffer.SizeInBytes / sizeof(int) < indicesList.Count)
            {
                dynTerrainIndexBuffer.Dispose();
                dynTerrainIndexBuffer = new DynamicIndexBuffer(device, typeof(int), indicesList.Count, BufferUsage.WriteOnly);
            }
            dynTerrainIndexBuffer.SetData(indicesList.ToArray(), 0, indicesList.Count, SetDataOptions.Discard);
        }

        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            device.RenderState.CullMode = CullMode.None;
            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            DrawTerrain();
            DrawOrthoGrid();
            device.Indices = null;

            base.Draw(gameTime);
        }

        private void DrawTerrain()
        {
            int width = heightData.GetLength(0);
            int height = heightData.GetLength(1);

            device.RenderState.FillMode = FillMode.Solid;
            device.RenderState.AlphaBlendEnable = false;

            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.Texture = grassTexture;
            basicEffect.TextureEnabled = true;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(terrainVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                device.Indices = dynTerrainIndexBuffer;
                device.VertexDeclaration = myVertexDeclaration;
                int noTriangles = triangleList.Count;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, width * height, 0, noTriangles);

                pass.End();
            }
            basicEffect.End();
        }

        private void DrawOrthoGrid()
        {
            int width = heightData.GetLength(0);
            int height = heightData.GetLength(1);

            device.RenderState.FillMode = FillMode.WireFrame;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = orthoView;
            basicEffect.Projection = orthoProj;
            basicEffect.TextureEnabled = false;

            device.RenderState.AlphaBlendEnable = true;
            float color = 0.4f;
            device.RenderState.BlendFactor = new Color(new Vector4(color, color, color, color));
            device.RenderState.SourceBlend = Blend.BlendFactor;
            device.RenderState.DestinationBlend = Blend.InverseBlendFactor;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(terrainVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                device.Indices = dynTerrainIndexBuffer;
                device.VertexDeclaration = myVertexDeclaration;
                int noTriangles = indicesList.Count / 3;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, width * height, 0, noTriangles);

                pass.End();
            }
            basicEffect.End();
            device.RenderState.AlphaBlendEnable = false;
        }
    }
}
