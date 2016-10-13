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

        Vector3 cameraPosition;
        Matrix cameraRotation;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            float viewAngle = MathHelper.PiOver4;
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            float nearPlane = 0.5f;
            float farPlane = 100.0f;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio,
            nearPlane, farPlane);

            cameraPosition = new Vector3(-5, 7, 14);
            cameraRotation = Matrix.CreateRotationX(-MathHelper.Pi / 8.0f) * Matrix.CreateRotationY(-MathHelper.Pi / 8.0f);

            UpdateViewMatrix();
            base.Initialize();
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MoveCameraForward();

            base.Update(gameTime);
        }

        private void MoveCameraForward()
        {
            float moveSpeed = 0.05f;            
            Vector3 cameraOriginalForward = new Vector3(0, 0, -1);
            Vector3 cameraRotatedForward = Vector3.Transform(cameraOriginalForward, cameraRotation);
            cameraPosition += moveSpeed * cameraRotatedForward;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

    
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            //render coordcross using specified View and Projection matrices
            cCross.Draw(viewMatrix, projectionMatrix);
            
            base.Draw(gameTime);
        }
    }
}