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
        Quaternion cameraRotation;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {            
            base.Initialize();

            float viewAngle = MathHelper.PiOver4;
            float aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            float nearPlane = 0.5f;
            float farPlane = 100.0f;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio,
            nearPlane, farPlane);

            cameraPosition = new Vector3(-1, 1, 10);
            cameraRotation = Quaternion.Identity;
            UpdateViewMatrix();
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
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float updownRotation = 0.0f;
            float leftrightRotation = 0.0f;

            leftrightRotation -= gamePadState.ThumbSticks.Left.X/50.0f;
            updownRotation += gamePadState.ThumbSticks.Left.Y/50.0f;

            KeyboardState keys = Keyboard.GetState();            

            if (keys.IsKeyDown(Keys.Up))
                updownRotation = 0.05f;
            if (keys.IsKeyDown(Keys.Down))
                updownRotation = -0.05f;
            if (keys.IsKeyDown(Keys.Right))
                leftrightRotation = -0.05f;
            if (keys.IsKeyDown(Keys.Left))
                leftrightRotation = 0.05f;

            Quaternion additionalRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), updownRotation) * Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRotation);
            cameraRotation = cameraRotation * additionalRotation;

            AddToCameraPosition(new Vector3(0, 0, -1));

            base.Update(gameTime);
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            float moveSpeed = 0.05f;
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
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

            //draw coordcross
            cCross.Draw(viewMatrix, projectionMatrix);
            
            base.Draw(gameTime);
        }
    }
}