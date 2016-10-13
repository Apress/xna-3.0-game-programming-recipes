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
        CoordCross cCross;
        QuatCam quatCam;

        Texture2D myTexture;
        VertexPositionTexture[] billboardVertices;
        VertexDeclaration myVertexDeclaration;
        List<Vector4> billboardList = new List<Vector4>();
        Effect bbEffect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            quatCam = new QuatCam(GraphicsDevice.Viewport);
            AddBillboards();
            CreateBBVertices();
            myVertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionTexture.VertexElements);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);

            myTexture = Content.Load<Texture2D>("billboardtexture");
            bbEffect = Content.Load<Effect>("bbEffect");
        }

        private void AddBillboards()
        {
            int CPUpower = 6;
            for (int x = -CPUpower; x < CPUpower; x++)
                for (int y = -CPUpower; y < CPUpower; y++)
                    for (int z = -CPUpower; z < CPUpower; z++)
                        billboardList.Add(new Vector4(x * 1, y * 1, z * 1, 10));
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

            quatCam.Update(mouseState, keyState, gamePadState);

            base.Update(gameTime);
        }

        private void CreateBBVertices()
        {
            billboardVertices = new VertexPositionTexture[billboardList.Count * 6];

            int i = 0;
            foreach (Vector4 currentV4 in billboardList)
            {
                Vector3 center = new Vector3(currentV4.X, currentV4.Y, currentV4.Z);

                billboardVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 0));
                billboardVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));

                billboardVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 0));
                billboardVertices[i++] = new VertexPositionTexture(center, new Vector2(1, 1));
                billboardVertices[i++] = new VertexPositionTexture(center, new Vector2(0, 1));
            }
        }
        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            cCross.Draw(quatCam.ViewMatrix, quatCam.ProjectionMatrix);

            //draw billboards
            bbEffect.CurrentTechnique = bbEffect.Techniques["SpheBillboard"];
            bbEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            bbEffect.Parameters["xProjection"].SetValue(quatCam.ProjectionMatrix);
            bbEffect.Parameters["xView"].SetValue(quatCam.ViewMatrix);

            bbEffect.Parameters["xCamPos"].SetValue(quatCam.Position);
            bbEffect.Parameters["xCamUp"].SetValue(quatCam.UpVector);
            bbEffect.Parameters["xBillboardTexture"].SetValue(myTexture);

            bbEffect.Begin();
            foreach (EffectPass pass in bbEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, billboardVertices, 0, billboardList.Count * 2);
                pass.End();
            }
            bbEffect.End();


            base.Draw(gameTime);
        }
    }
}
