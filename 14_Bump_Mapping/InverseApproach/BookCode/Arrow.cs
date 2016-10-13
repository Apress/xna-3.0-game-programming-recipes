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
    class Arrow
    {
        private VertexPositionColor[] vertices;
        private GraphicsDevice device;
        private BasicEffect basicEffect;

        public Arrow(GraphicsDevice device)
        {
            this.device = device;
            basicEffect = new BasicEffect(device, null);

            InitVertices();
        }

        private void InitVertices()
        {
            vertices = new VertexPositionColor[30];

            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[1] = new VertexPositionColor(Vector3.Up * 5, Color.White);
            vertices[2] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[3] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0), Color.White);
            vertices[4] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            vertices[5] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0), Color.White);
        }

        public void Draw(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.VertexColorEnabled = true;
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.VertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 3);           

                pass.End();
            }
            basicEffect.End();            
        }
    }
}
