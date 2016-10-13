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
    class BillboardGC : DrawableGameComponent
    {
        private GraphicsDevice device;
        private ICameraInterface camera;

        private BasicEffect basicEffect;
        private Texture2D myTexture;
        private VertexPositionTexture[] billboardVertices;
        private VertexDeclaration myVertexDeclaration;
        private List<Vector4> billboardList = new List<Vector4>();

        private Vector3 camPosition;
        private Vector3 camForward;
        private Matrix viewMatrix;
        private Matrix projectionMatrix;

        public BillboardGC(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            device = Game.GraphicsDevice;
            camera = (ICameraInterface)Game.Services.GetService(typeof(ICameraInterface));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            basicEffect = new BasicEffect(device, null);
            myTexture = Game.Content.Load<Texture2D>("billboardtexture");
            AddBillboards();
            myVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements); 	
        }

        private void AddBillboards()
        {
            int CPUpower = 6;
            for (int x = -CPUpower; x < CPUpower; x++)
                for (int y = -CPUpower; y < CPUpower; y++)
                    for (int z = -CPUpower; z < CPUpower; z++)
                        billboardList.Add(new Vector4(x * 20, y * 20, z * 20, 10));
        }

        public override void Update(GameTime gameTime)
        {
            camPosition = camera.Position;
            camForward = camera.Forward;

            CreateBBVertices();

            base.Update(gameTime);
        }

        private void CreateBBVertices()
        {
            billboardVertices = new VertexPositionTexture[billboardList.Count * 6];

            int i = 0;
            foreach (Vector4 currentV4 in billboardList)
            {
                Vector3 center = new Vector3(currentV4.X, currentV4.Y, currentV4.Z);
                float scaling = currentV4.W;

                Matrix bbMatrix = Matrix.CreateConstrainedBillboard(center, camPosition, new Vector3(0, 1, 0), camForward, null);

                //first triangle
                Vector3 posDL = new Vector3(-0.5f, -0.5f, 0);
                Vector3 billboardedPosDL = Vector3.Transform(posDL * scaling, bbMatrix);
                billboardVertices[i++] = new VertexPositionTexture(billboardedPosDL, new Vector2(1, 1));
                Vector3 posUR = new Vector3(0.5f, 0.5f, 0);
                Vector3 billboardedPosUR = Vector3.Transform(posUR * scaling, bbMatrix);
                billboardVertices[i++] = new VertexPositionTexture(billboardedPosUR, new Vector2(0, 0));
                Vector3 posUL = new Vector3(-0.5f, 0.5f, 0);
                Vector3 billboardedPosUL = Vector3.Transform(posUL * scaling, bbMatrix);
                billboardVertices[i++] = new VertexPositionTexture(billboardedPosUL, new Vector2(1, 0));

                //second triangle: 2 of 3 cornerpoints already calculated!
                billboardVertices[i++] = new VertexPositionTexture(billboardedPosDL, new Vector2(1, 1));
                Vector3 posDR = new Vector3(0.5f, -0.5f, 0);
                Vector3 billboardedPosDR = Vector3.Transform(posDR * scaling, bbMatrix);
                billboardVertices[i++] = new VertexPositionTexture(billboardedPosDR, new Vector2(0, 1));
                billboardVertices[i++] = new VertexPositionTexture(billboardedPosUR, new Vector2(0, 0));
            }
        }

        public override void Draw(GameTime gameTime)
        {
            //draw billboards
            basicEffect.World = Matrix.Identity;
            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = camera.ProjectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = myTexture;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, billboardVertices, 0, billboardList.Count * 2);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
