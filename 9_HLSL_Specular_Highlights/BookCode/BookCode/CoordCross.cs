using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
    class CoordCross
    {
        private VertexPositionColor[] vertices;
        private GraphicsDevice device;
        private BasicEffect basicEffect;
        private VertexDeclaration vertDeclaration;

        public CoordCross(GraphicsDevice device)
        {
            this.device = device;
            basicEffect = new BasicEffect(device, null);

            InitVertices();
            vertDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
        }

        private void InitVertices()
        {
            vertices = new VertexPositionColor[30];

            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[1] = new VertexPositionColor(Vector3.Right * 5, Color.White);
            vertices[2] = new VertexPositionColor(new Vector3(5, 0, 0), Color.White);
            vertices[3] = new VertexPositionColor(new Vector3(4.5f, 0.5f, 0), Color.White);
            vertices[4] = new VertexPositionColor(new Vector3(5, 0, 0), Color.White);
            vertices[5] = new VertexPositionColor(new Vector3(4.5f, -0.5f, 0), Color.White);

            vertices[6] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[7] = new VertexPositionColor(Vector3.Up * 5, Color.White);
            vertices[8] = new VertexPositionColor(new Vector3(0, 5, 0), Color.White);
            vertices[9] = new VertexPositionColor(new Vector3(0.5f, 4.5f, 0), Color.White);
            vertices[10] = new VertexPositionColor(new Vector3(0, 5, 0), Color.White);
            vertices[11] = new VertexPositionColor(new Vector3(-0.5f, 4.5f, 0), Color.White);

            vertices[12] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[13] = new VertexPositionColor(Vector3.Forward * 5, Color.White);
            vertices[14] = new VertexPositionColor(new Vector3(0, 0, -5), Color.White);
            vertices[15] = new VertexPositionColor(new Vector3(0, 0.5f, -4.5f), Color.White);
            vertices[16] = new VertexPositionColor(new Vector3(0, 0, -5), Color.White);
            vertices[17] = new VertexPositionColor(new Vector3(0, -0.5f, -4.5f), Color.White);
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            basicEffect.World = Matrix.Identity;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.VertexColorEnabled = true;
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.VertexDeclaration = vertDeclaration;
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 9);           

                pass.End();
            }
            basicEffect.End();            
        }

        public void DrawUsingPresetEffect()
        {
            device.VertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
            device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 9);           
        }
    }
}
