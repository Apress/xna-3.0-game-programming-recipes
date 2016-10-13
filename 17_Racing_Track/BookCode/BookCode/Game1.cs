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
        VertexDeclaration myVertexDeclaration;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(7, 10, 7), MathHelper.PiOver4, -MathHelper.PiOver4);

            List<Vector3> basePoints = new List<Vector3>();
            basePoints.Add(new Vector3(2, 0, 4));
            basePoints.Add(new Vector3(0, 0, 0));
            basePoints.Add(new Vector3(-2, 0, -4));
            basePoints.Add(new Vector3(2, 0, -4));
            basePoints.Add(new Vector3(0, 1, 0));
            basePoints.Add(new Vector3(-2, 0, 4));

            List<Vector3> extendedTrackPoints = GenerateTrackPoints(basePoints);
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

        private VertexPositionNormalTexture[] GenerateTrackVertices(List<Vector3> basePoints)
        {
            float halfTrackWidth = 0.2f;
            float textureLenght = 0.5f;

            float distance = 0;
            List<VertexPositionNormalTexture> verticesList = new List<VertexPositionNormalTexture>();

            for (int i = 1; i < basePoints.Count - 1; i++)
            {
                Vector3 carDir = basePoints[i + 1] - basePoints[i];
                Vector3 sideDir = Vector3.Cross(new Vector3(0, 1, 0), carDir);
                sideDir.Normalize();

                Vector3 outerPoint = basePoints[i] + sideDir * halfTrackWidth;
                Vector3 innerPoint = basePoints[i] - sideDir * halfTrackWidth;

                VertexPositionNormalTexture vertex;
                vertex = new VertexPositionNormalTexture(innerPoint, new Vector3(0, 1, 0), new Vector2(0, distance / textureLenght));
                verticesList.Add(vertex);
                vertex = new VertexPositionNormalTexture(outerPoint, new Vector3(0, 1, 0), new Vector2(1, distance / textureLenght));
                verticesList.Add(vertex);
                distance += carDir.Length();
            }

            VertexPositionNormalTexture extraVert = verticesList[0];
            extraVert.TextureCoordinate.Y = distance / textureLenght;
            verticesList.Add(extraVert);

            extraVert = verticesList[1];
            extraVert.TextureCoordinate.Y = distance / textureLenght;
            verticesList.Add(extraVert);

            return verticesList.ToArray();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);
            road = Content.Load<Texture2D>("road");
            myVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
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
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, trackVertices, 0, trackVertices.Length - 2);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
