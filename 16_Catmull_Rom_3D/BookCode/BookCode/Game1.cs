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

        VertexPositionColor[] straightVertices;
        VertexPositionColor[] crVertices1;
        VertexPositionColor[] crVertices2;
        VertexPositionColor[] crVertices3;
        VertexDeclaration myVertexDeclaration;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private Vector3 CR3D(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float amount)
        {
            Vector3 result = new Vector3();

            result.X = MathHelper.CatmullRom(v1.X, v2.X, v3.X, v4.X, amount);
            result.Y = MathHelper.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, amount);
            result.Z = MathHelper.CatmullRom(v1.Z, v2.Z, v3.Z, v4.Z, amount);

            return result;
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(5, 2, 15), 0, 0);

            List<Vector3> points = new List<Vector3>();

            points.Add(new Vector3(0, 0, 0));
            points.Add(new Vector3(2, 2, 0));
            points.Add(new Vector3(4, 0, 0));
            points.Add(new Vector3(6, 6, 0));
            points.Add(new Vector3(8, 2, 0));
            points.Add(new Vector3(10, 0, 0));

            List<Vector3> crList1 = InterpolateCR(points[0], points[1], points[2], points[3]);
            List<Vector3> crList2 = InterpolateCR(points[1], points[2], points[3], points[4]);
            List<Vector3> crList3 = InterpolateCR(points[2], points[3], points[4], points[5]);

            straightVertices = XNAUtils.VerticesFromVector3List(points, Color.Red);
            crVertices1 = XNAUtils.VerticesFromVector3List(crList1, Color.Green);
            crVertices2 = XNAUtils.VerticesFromVector3List(crList2, Color.Blue);
            crVertices3 = XNAUtils.VerticesFromVector3List(crList3, Color.Yellow);

            base.Initialize();
        }

        private List<Vector3> InterpolateCR(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            List<Vector3> list = new List<Vector3>();
            int detail = 20;
            for (int i = 0; i < detail; i++)
            {
                Vector3 newPoint = CR3D(v1, v2, v3, v4, (float)i / (float)detail);
                list.Add(newPoint);
            }
            list.Add(v3);
            return list;
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);
            myVertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
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

            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            //draw primitives
            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, straightVertices, 0, straightVertices.Length - 1);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, crVertices1, 0, crVertices1.Length - 1);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, crVertices2, 0, crVertices2.Length - 1);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, crVertices3, 0, crVertices3.Length - 1);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
