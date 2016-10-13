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
        CoordCross cCross;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        float leftrightRot;
        float updownRot;
        Vector3 cameraPosition;
#if XBOX
#else
        MouseState originalMouseState;
#endif

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            base.Initialize();

            float viewAngle = MathHelper.PiOver4;
            float aspectRatio = GraphicsDevice.Viewport.AspectRatio;
            float nearPlane = 0.5f;
            float farPlane = 100.0f;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio,
            nearPlane, farPlane);

            leftrightRot = 0.0f;
            updownRot = 0.0f;
            cameraPosition = new Vector3(1,1,10);
            UpdateViewMatrix();

#if XBOX
#else
            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            originalMouseState = Mouse.GetState();
#endif
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            cCross = new CoordCross(device);     
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float rotationSpeed = 0.005f;

#if XBOX
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            leftrightRot -= rotationSpeed* gamePadState.ThumbSticks.Left.X*5.0f;
            updownRot += rotationSpeed * gamePadState.ThumbSticks.Left.Y;

            UpdateViewMatrix();
            
            AddToCameraPosition(new Vector3(gamePadState.ThumbSticks.Right.X, 0, -gamePadState.ThumbSticks.Right.Y));
#else
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {                
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference;
                updownRot -= rotationSpeed * yDifference;
                Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);               

                UpdateViewMatrix();                
            }
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up))
                AddToCameraPosition(new Vector3(0, 0, -1));
            if (keyState.IsKeyDown(Keys.Down))
                AddToCameraPosition(new Vector3(0, 0, 1));
            if (keyState.IsKeyDown(Keys.Right))
                AddToCameraPosition(new Vector3(1, 0, 0));
            if (keyState.IsKeyDown(Keys.Left))
                AddToCameraPosition(new Vector3(-1, 0, 0));
#endif
            base.Update(gameTime);
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            float moveSpeed = 0.5f;
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) *Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);
            Vector3 cameraFinalUpVector = cameraPosition + cameraRotatedUpVector;

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);
        }
    
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            //draw coordcross
            cCross.Draw(viewMatrix, projectionMatrix);
            
            base.Draw(gameTime);
        }
    }
}