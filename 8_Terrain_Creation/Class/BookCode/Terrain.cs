using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
    public class Terrain
    {
        private GraphicsDevice device;
        private ContentManager content;
        private BasicEffect basicEffect;

        private Texture2D grassTexture;
        private float[,] heightData;
        private VertexBuffer terrainVertexBuffer;
        private IndexBuffer terrainIndexBuffer;
        private VertexDeclaration terrainVertexDeclaration;
        
        public Terrain(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            this.content = content;
            basicEffect = new BasicEffect(device, null);
            
            Texture2D heightMap = content.Load<Texture2D>("heightmap");                                    
            LoadHeightData(heightMap);

            grassTexture = content.Load<Texture2D>("grass");
            
            VertexPositionNormalTexture[] terrainVertices = CreateTerrainVertices();
            int[] terrainIndices = CreateTerrainIndices();
            terrainVertices = GenerateNormalsForTriangleStrip(terrainVertices, terrainIndices);
            CreateBuffers(terrainVertices, terrainIndices);

            terrainVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
        }

        //constructor allowing a Terrain to be created without having a link to the device
        public Terrain(VertexBuffer vb, IndexBuffer ib, BasicEffect basicEffect, Texture2D grassTexture, float[,] heightData)
        {
            terrainVertexBuffer = vb;
            terrainIndexBuffer = ib;
            this.basicEffect = basicEffect;
            this.grassTexture = grassTexture;
            this.heightData = heightData;
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            float minimumHeight = 255;
            float maximumHeight = 0;

            int width = heightMap.Width;
            int height = heightMap.Height;

            Color[] heightMapColors = new Color[width * height];
            heightMap.GetData<Color>(heightMapColors);

            heightData = new float[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * width].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
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
            
            int[] terrainIndices = new int[(width)*2*(height-1)];
            
            int i = 0;
            int z = 0;
            while (z < height-1)
            {
                for (int x = 0; x < width; x++)
                {
                    terrainIndices[i++] = x + z * width;
                    terrainIndices[i++] = x + (z + 1) * width;
                }
                z++;

                if (z < height-1)
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

        private void CreateBuffers(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            terrainVertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(vertices);

            terrainIndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);
        }

        public void Draw(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            int width = heightData.GetLength(0);
            int height = heightData.GetLength(1);

            GraphicsDevice device = terrainVertexBuffer.GraphicsDevice;

            //draw terrain            
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.Texture = grassTexture;
            basicEffect.TextureEnabled = true;
            
            basicEffect.EnableDefaultLighting();
            Vector3 lightDirection = new Vector3(1, -1, 1);
            lightDirection.Normalize();
            basicEffect.DirectionalLight0.Direction = lightDirection;
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            basicEffect.DirectionalLight1.Enabled = false;
            basicEffect.DirectionalLight2.Enabled = false;
            basicEffect.SpecularColor = new Vector3(0, 0, 0);
            
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(terrainVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                device.Indices = terrainIndexBuffer;
                device.VertexDeclaration = terrainVertexDeclaration;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, width * height, 0,width*2*(height-1)-2);

                pass.End();
            }
            basicEffect.End();
        }

        private void DrawWithCustomEffect()
        {
            int width = heightData.GetLength(0);
            int height = heightData.GetLength(1);

            device.Vertices[0].SetSource(terrainVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.Indices = terrainIndexBuffer;
            device.VertexDeclaration = terrainVertexDeclaration;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, width * height, 0, width * 2 * (height - 1) - 2);
        }
        
        public float GetClippedHeightAt(float x, float z)
        {
            bool invalid = x < 0;
            invalid |= z < 0;
            invalid |= x > heightData.GetLength(0) - 1;
            invalid |= z > heightData.GetLength(1) - 1;
            if (invalid)
                return 10;            
            else
                return heightData[(int)x, (int)z];
        }

        public float GetExactHeightAt(float xCoord, float zCoord)
        {
            bool invalid = xCoord < 0;
            invalid |= zCoord < 0;
            invalid |= xCoord >= heightData.GetLength(0) - 1;
            invalid |= zCoord >= heightData.GetLength(1) - 1;
            if (invalid)
                return 10;
                        
            int xLower = (int)xCoord;
            int xHigher = xLower + 1;
            float xRelative = (xCoord - xLower) / ((float)xHigher - (float)xLower);

            int zLower = (int)zCoord;
            int zHigher = zLower + 1;
            float zRelative = (zCoord - zLower) / ((float)zHigher - (float)zLower);

            float heightLxLz = heightData[xLower, zLower];
            float heightLxHz = heightData[xLower, zHigher];
            float heightHxLz = heightData[xHigher, zLower];
            float heightHxHz = heightData[xHigher, zHigher];

            bool cameraAboveLowerTriangle = (xRelative + zRelative < 1);

            float finalHeight;
            if (cameraAboveLowerTriangle)
            {
                finalHeight = heightLxLz;
                finalHeight += zRelative * (heightLxHz - heightLxLz);
                finalHeight += xRelative * (heightHxLz - heightLxLz);
            }
            else
            {
                finalHeight = heightHxHz;
                finalHeight += (1.0f - zRelative) * (heightHxLz - heightHxHz);
                finalHeight += (1.0f - xRelative) * (heightLxHz - heightHxHz);
            }

            return finalHeight;
        }
    }
}
