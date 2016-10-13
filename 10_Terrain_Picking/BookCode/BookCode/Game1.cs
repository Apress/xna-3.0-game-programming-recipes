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
        Vector3 pointerPos;
        Vector2 pointerScreenPos;
        Texture2D crosshair;
        SpriteBatch spriteBatch;

        Terrain terrain;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(-10, 50, 10), -MathHelper.PiOver4, -MathHelper.Pi/8.0f);
            pointerScreenPos = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            spriteBatch = new SpriteBatch(device);
            basicEffect = new BasicEffect(device, null);            
            cCross = new CoordCross(device);
            crosshair = Content.Load<Texture2D>("cross");

            terrain = new Terrain(device, Content);            
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

#if XBOX
            pointerScreenPos.X += gamePadState.ThumbSticks.Left.X*2.0f;
            pointerScreenPos.Y -= gamePadState.ThumbSticks.Left.Y*2.0f;
#else
            pointerScreenPos = new Vector2(mouseState.X, mouseState.Y);
#endif
            Ray pointerRay = GetPointerRay(pointerScreenPos);

            Ray clippedRay = ClipRay(pointerRay, 30, 0);
            Ray shorterRay = LinearSearch(clippedRay);
            pointerPos = BinarySearch(shorterRay);
            
            base.Update(gameTime);
        }

        private Ray GetPointerRay(Vector2 pointerPosition)
        {
            Vector3 nearScreenPoint = new Vector3(pointerPosition.X, pointerPosition.Y, 0);
            Vector3 farScreenPoint = new Vector3(pointerPosition.X, pointerPosition.Y, 1);

            Vector3 near3DWorldPoint = device.Viewport.Unproject(nearScreenPoint, fpsCam.ProjectionMatrix, fpsCam.ViewMatrix, Matrix.Identity);
            Vector3 far3DWorldPoint = device.Viewport.Unproject(farScreenPoint, fpsCam.ProjectionMatrix, fpsCam.ViewMatrix, Matrix.Identity);

            Vector3 pointerRayDirection = far3DWorldPoint - near3DWorldPoint;
            Ray pointerRay = new Ray(near3DWorldPoint, pointerRayDirection);

            return pointerRay;
        }

        private Ray ClipRay(Ray ray, float highest, float lowest)
        {
            Vector3 oldStartPoint = ray.Position;

            float factorH = -(oldStartPoint.Y-highest) / ray.Direction.Y;
            Vector3 pointA = oldStartPoint + factorH * ray.Direction;

            float factorL = -(oldStartPoint.Y-lowest) / ray.Direction.Y;
            Vector3 pointB = oldStartPoint + factorL * ray.Direction;

            Vector3 newDirection = pointB - pointA;
            return new Ray(pointA, newDirection);
        }

        private Ray LinearSearch(Ray ray)
        {
            ray.Direction /= 50.0f;

            Vector3 nextPoint = ray.Position + ray.Direction;
            float heightAtNextPoint = terrain.GetExactHeightAt(nextPoint.X, -nextPoint.Z);
            while (heightAtNextPoint < nextPoint.Y)
            {
                ray.Position = nextPoint;

                nextPoint = ray.Position + ray.Direction;
                heightAtNextPoint = terrain.GetExactHeightAt(nextPoint.X, -nextPoint.Z);
            }
            return ray;
        }

        private Vector3 BinarySearch(Ray ray)
        {
            float accuracy = 0.01f;
            float heightAtStartingPoint = terrain.GetExactHeightAt(ray.Position.X, -ray.Position.Z);
            float currentError = ray.Position.Y - heightAtStartingPoint;
            int counter = 0;
            while (currentError > accuracy)
            {
                ray.Direction /= 2.0f;
                Vector3 nextPoint = ray.Position + ray.Direction;
                float heightAtNextPoint = terrain.GetExactHeightAt(nextPoint.X, -nextPoint.Z);
                if (nextPoint.Y > heightAtNextPoint)
                {
                    ray.Position = nextPoint;
                    currentError = ray.Position.Y - heightAtNextPoint;
                }
                if (counter++ == 1000) break;
            }
            return ray.Position;
        }

        protected override void Draw(GameTime gameTime)
        {
            float lowestHeight = 0;
            float highestHeight = 30;
            float interp = (pointerPos.Y - lowestHeight) / (highestHeight - lowestHeight);
            byte colorValue = (byte)(255.0f * interp);
            Color clearColor = new Color(colorValue, colorValue, colorValue);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearColor, 1, 0);

            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            //draw terrain
            terrain.Draw(Matrix.Identity, fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Draw(crosshair, pointerScreenPos, null, Color.White, 0, new Vector2(7, 7), 1, SpriteEffects.None, 0);
            spriteBatch.End();

            Window.Title = pointerPos.Y.ToString();

            base.Draw(gameTime);
        }
    }
}
