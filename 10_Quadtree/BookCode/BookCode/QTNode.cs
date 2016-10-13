using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BookCode
{
    class QTNode
    {
        private int width;
        private int height;
        private bool isEndNode;
        private BoundingBox nodeBoundingBox;
        private GraphicsDevice device;
        private BasicEffect basicEffect;

        private QTNode nodeUL;
        private QTNode nodeUR;
        private QTNode nodeLL;
        private QTNode nodeLR;

        private VertexBuffer nodeVertexBuffer;
        private IndexBuffer nodeIndexBuffer;
        private Texture2D grassTexture;

        public static int NodesRendered;

        public QTNode(VertexPositionNormalTexture[,] vertexArray, GraphicsDevice device, Texture2D grassTexture, int maxSize)
        {
            this.device = device;
            this.grassTexture = grassTexture;
            basicEffect = new BasicEffect(device, null);

            width = vertexArray.GetLength(0);
            height = vertexArray.GetLength(1);
            nodeBoundingBox = CreateBoundingBox(vertexArray);

            isEndNode = width <= maxSize;
            isEndNode &= height <= maxSize;
            if (isEndNode)
            {
                VertexPositionNormalTexture[] vertices = Reshape2Dto1D<VertexPositionNormalTexture>(vertexArray);
                int[] indices = TerrainUtils.CreateTerrainIndices(width, height);
                TerrainUtils.CreateBuffers(vertices, indices, out nodeVertexBuffer, out nodeIndexBuffer, device);
            }
            else
            {
                CreateChildNodes(vertexArray, maxSize);
            }
        }

        private BoundingBox CreateBoundingBox(VertexPositionNormalTexture[,] vertexArray)
        {
            List<Vector3> pointList = new List<Vector3>();
            foreach (VertexPositionNormalTexture vertex in vertexArray)
                pointList.Add(vertex.Position);
            
            BoundingBox nodeBoundingBox = BoundingBox.CreateFromPoints(pointList);
            return nodeBoundingBox;
        }

        private void CreateChildNodes(VertexPositionNormalTexture[,] vertexArray, int maxSize)
        {
            VertexPositionNormalTexture[,] ulArray = new VertexPositionNormalTexture[width / 2 + 1, height / 2 + 1];
            for (int w = 0; w < width / 2 + 1; w++)
                for (int h = 0; h < height / 2 + 1; h++)
                    ulArray[w, h] = vertexArray[w, h];
            nodeUL = new QTNode(ulArray, device, grassTexture, maxSize);

            VertexPositionNormalTexture[,] urArray = new VertexPositionNormalTexture[width - (width / 2), height / 2 + 1];
            for (int w = 0; w < width - (width / 2); w++)
                for (int h = 0; h < height / 2 + 1; h++)
                    urArray[w, h] = vertexArray[width / 2 + w, h];
            nodeUR = new QTNode(urArray, device, grassTexture, maxSize);

            VertexPositionNormalTexture[,] llArray = new VertexPositionNormalTexture[width / 2 + 1, height - (height / 2)];
            for (int w = 0; w < width / 2 + 1; w++)
                for (int h = 0; h < height - (height / 2); h++)
                    llArray[w, h] = vertexArray[w, height / 2 + h];
            nodeLL = new QTNode(llArray, device, grassTexture, maxSize);

            VertexPositionNormalTexture[,] lrArray = new VertexPositionNormalTexture[width - (width / 2), height - (height / 2)];
            for (int w = 0; w < width - (width / 2); w++)
                for (int h = 0; h < height - (height / 2); h++)
                    lrArray[w, h] = vertexArray[width / 2 + w, height / 2 + h];
            nodeLR = new QTNode(lrArray, device, grassTexture, maxSize);
        }

        private T[] Reshape2Dto1D<T>(T[,] array2D)
        {
            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);
            T[] array1D = new T[width * height];

            int i = 0;
            for (int z = 0; z < height; z++)
                for (int x = 0; x < width; x++)
                    array1D[i++] = array2D[x, z];

            return array1D;
        }                

        private void DrawCurrentNode(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            //device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            //device.RenderState.FillMode = FillMode.WireFrame;

            NodesRendered++;
            
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.Texture = grassTexture;
            basicEffect.VertexColorEnabled = false;
            basicEffect.TextureEnabled = true;

            basicEffect.EnableDefaultLighting();
            basicEffect.DirectionalLight0.Direction = new Vector3(1, -1, 1);
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            basicEffect.DirectionalLight1.Enabled = false;
            basicEffect.DirectionalLight2.Enabled = false;
            basicEffect.SpecularColor = new Vector3(0, 0, 0);

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.Vertices[0].SetSource(nodeVertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                device.Indices = nodeIndexBuffer;
                device.VertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, width * height, 0, (width * 2 * (height - 1) - 2));

                pass.End();
            }
            basicEffect.End();

            //XNAUtils.DrawBoundingBox(nodeBoundingBox, device, basicEffect, worldMatrix, viewMatrix, projectionMatrix);
        }

        public void Draw(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, BoundingFrustum cameraFrustum)
        {
            BoundingBox transformedBBox = XNAUtils.TransformBoundingBox(nodeBoundingBox, worldMatrix);
            ContainmentType cameraNodeContainment = cameraFrustum.Contains(transformedBBox);
            if (cameraNodeContainment != ContainmentType.Disjoint)
            {
                if (isEndNode)
                {
                    DrawCurrentNode(worldMatrix, viewMatrix, projectionMatrix);
                }
                else
                {
                    nodeUL.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustum);
                    nodeUR.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustum);
                    nodeLL.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustum);
                    nodeLR.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustum);
                }
            }
        }
    }
}
