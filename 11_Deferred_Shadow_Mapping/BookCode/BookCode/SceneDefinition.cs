using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

    partial class Game1
    {
        private void InitSceneVertices()
        {
            wallVertices = new VertexPositionNormalTexture[16];

            int i = 0;

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 10, 30), new Vector3(1, 0, 0), new Vector2(0, 0));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 10, -30), new Vector3(1, 0, 0), new Vector2(10, 0));

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 0, 30), new Vector3(1, 0, 0), new Vector2(0, 1));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 0, -30), new Vector3(1, 0, 0), new Vector2(10, 1));

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 0, 30), new Vector3(0, 1, 0), new Vector2(0.5f, 0.5f));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 0, -30), new Vector3(0, 1, 0), new Vector2(0.5f, 0.5f));

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 0, 30), new Vector3(0, 1, 0), new Vector2(0.5f, 0.5f));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 0, -30), new Vector3(0, 1, 0), new Vector2(0.5f, 0.5f));

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 0, 30), new Vector3(-1, 0, 0), new Vector2(10, 1));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 0, -30), new Vector3(-1, 0, 0), new Vector2(0, 1));

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 10, 30), new Vector3(-1, 0, 0), new Vector2(10, 0));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 10, -30), new Vector3(-1, 0, 0), new Vector2(0, 0));

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 10, -30), new Vector3(0, 0, 1), new Vector2(10, 0));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(30, 0, -30), new Vector3(0, 0, 1), new Vector2(10, 1));

            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 10, -30), new Vector3(0, 0, 1), new Vector2(0, 0));
            wallVertices[i++] = new VertexPositionNormalTexture(new Vector3(-30, 0, -30), new Vector3(0, 0, 1), new Vector2(0, 1));

            wallVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
        }

        private VertPosTexNormTan[] InitTowerVertices()
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
                posLow.Y = 0 / 2.0f;

                Vector3 posHigh = posLow;
                posHigh.Y += height;

                Vector2 texCoordLow = new Vector2(angle / (MathHelper.Pi * 2.0f), 1);
                Vector2 texCoordHigh = new Vector2(angle / (MathHelper.Pi * 2.0f), 0);

                verticesList.Add(new VertPosTexNormTan(posLow, texCoordLow, Vector3.Zero, new Vector3(0, 1, 0)));
                verticesList.Add(new VertPosTexNormTan(posHigh, texCoordHigh, Vector3.Zero, new Vector3(0, 1, 0)));
            }
            
            return verticesList.ToArray();
        }

        private int[] InitTowerIndices(VertPosTexNormTan[] vertices)
        {
            int triangles = vertices.Length - 2;
            int[] indices = new int[triangles * 3];

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

        private VertPosTexNormTan[] GenerateNormalsForTriangleList(VertPosTexNormTan[] verts, int[] indices)
        {
            for (int i = 0; i < verts.Length; i++)
                verts[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                Vector3 firstVec = verts[indices[i * 3 + 1]].Position - verts[indices[i * 3]].Position;
                Vector3 secondVec = verts[indices[i * 3 + 2]].Position - verts[indices[i * 3]].Position;
                Vector3 normal = Vector3.Cross(secondVec, firstVec);
                normal.Normalize();

                if (!float.IsNaN(normal.X))
                {
                    verts[indices[i * 3]].Normal += normal;
                    verts[indices[i * 3 + 1]].Normal += normal;
                    verts[indices[i * 3 + 2]].Normal += normal;
                }
            }

            for (int i = 0; i < verts.Length; i++)
                verts[i].Normal.Normalize();

            return verts;
        }
    }
}
