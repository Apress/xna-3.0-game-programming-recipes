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

        Texture2D road;
        VertexPositionNormalTexture[] trackVertices;
        VertexDeclaration trackVertexDeclaration;
        VertexPositionColor[] supportVertices;
        VertexDeclaration supportVertexDeclaration;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(10, 6, 9), MathHelper.Pi/8.0f*1.0f, -MathHelper.Pi/8.0f*1.0f);

            List<Vector3> trackPoints = new List<Vector3>();
            trackPoints.Add(new Vector3(0, 0, -2));

            trackPoints.Add(new Vector3(7, 0, 1));
            trackPoints.Add(new Vector3(7, 3, 1));
            trackPoints.Add(new Vector3(4, 3, 0));
            trackPoints.Add(new Vector3(4, 0, 0));

            trackPoints.Add(new Vector3(10, 0, 0));
            trackPoints.Add(new Vector3(8, 0, -4));
            trackPoints.Add(new Vector3(12, 0, -7));
            trackPoints.Add(new Vector3(8, 1, -10));
            trackPoints.Add(new Vector3(12, 0, -14));
            trackPoints.Add(new Vector3(6, 0, -17));

            trackPoints.Add(new Vector3(2, 1, -13));
            trackPoints.Add(new Vector3(9, 2, -6));
            trackPoints.Add(new Vector3(4, 1, 2));
            trackPoints.Add(new Vector3(1, 1, 2));

            trackPoints.Add(new Vector3(3, 0, -3));
            trackPoints.Add(new Vector3(9, 0, -10));
            trackPoints.Add(new Vector3(6, 0, -11));

            List<Vector3> extendedTrackPoints = GenerateTrackPoints(trackPoints);
            trackVertices = GenerateTrackVertices(extendedTrackPoints);

            base.Initialize();
        }

        private Vector3 CR3D(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float amount)
        {
            Vector3 result = new Vector3();

            result.X = MathHelper.CatmullRom(v1.X, v2.X, v3.X, v4.X, amount);
            result.Y = MathHelper.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, amount);
            result.Z = MathHelper.CatmullRom(v1.Z, v2.Z, v3.Z, v4.Z, amount);

            return result;
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
            return list;
        }

        private List<Vector3> GenerateTrackPoints(List<Vector3> basePoints)
        {
            basePoints.Add(basePoints[0]);
            basePoints.Add(basePoints[1]);
            basePoints.Add(basePoints[2]);

            List<Vector3> allPoints = new List<Vector3>();

            for (int i = 1; i < basePoints.Count - 2; i++)
            {
                List<Vector3> part = InterpolateCR(basePoints[i - 1], basePoints[i], basePoints[i + 1], basePoints[i + 2]);
                allPoints.AddRange(part);
            }

            return allPoints;
        }

        private VertexPositionNormalTexture[] GenerateTrackVertices(List<Vector3> trackPoints)
        {
            float halfTrackWidth = 0.2f;
            List<VertexPositionNormalTexture> verticesList = new List<VertexPositionNormalTexture>();
            List<Vector3> supportList = new List<Vector3>();

            float distance = 0;
            float textureLenght = 0.5f;

            Vector3 currentNormal = Vector3.Up;
            for (int i = 1; i < trackPoints.Count - 1; i++)
            {
                Vector3 next2 = trackPoints[i + 1] - trackPoints[i];
                next2.Normalize();
                Vector3 previous = trackPoints[i] - trackPoints[i - 1];
                previous.Normalize();

                Vector3 split = Vector3.Cross(next2, previous);
                Vector3 mySide = Vector3.Cross(next2, split);

                currentNormal = currentNormal + 0.2f * Vector3.Up + mySide * 2.0f;

                supportList.Add(new Vector3(trackPoints[i].X, -0.5f, trackPoints[i].Z));
                supportList.Add(trackPoints[i]);

                Vector3 side = Vector3.Cross(currentNormal, next2);
                side.Normalize();
                currentNormal = Vector3.Cross(next2, side);

                Vector3 outerPoint = trackPoints[i] + side * halfTrackWidth;
                Vector3 innerPoint = trackPoints[i] - side * halfTrackWidth;

                distance += next2.Length();
                VertexPositionNormalTexture vertex;
                vertex = new VertexPositionNormalTexture(innerPoint, currentNormal, new Vector2(0, distance / textureLenght));
                verticesList.Add(vertex);
                vertex = new VertexPositionNormalTexture(outerPoint, currentNormal, new Vector2(1, distance / textureLenght));
                verticesList.Add(vertex);
            }
            verticesList.Add(verticesList[0]);
            verticesList.Add(verticesList[1]);

            supportList.Add(Vector3.Zero);
            supportList.Add(Vector3.Zero);
            supportVertices = XNAUtils.VerticesFromVector3List(supportList, Color.Yellow);

            VertexPositionNormalTexture[] trackVertices = verticesList.ToArray();

            return trackVertices;
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);
            road = Content.Load<Texture2D>("road");
            trackVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
            supportVertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
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

            //render track
            device.RenderState.CullMode = CullMode.None;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;

            basicEffect.Texture = road;
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = false;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = trackVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, trackVertices, 0, trackVertices.Length - 2);
                pass.End();
            }
            basicEffect.End();

            //render supports
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = supportVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, supportVertices, 0, supportVertices.Length / 2);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
