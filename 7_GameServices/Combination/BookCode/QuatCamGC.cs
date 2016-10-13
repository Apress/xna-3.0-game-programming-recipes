using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
#if XBOX
    //When compiled for the XBox, Mouse and MouseState are unknown.
    //Define dummy bodies for them, so a MouseState object
    //can be passed to the Update method of the camera.
    public class MouseState
    {
    }
    public static class Mouse
    {
        public static MouseState GetState()
        {
            return new MouseState();
        }
    }
#endif    

    class QuatCamGC : GameComponent, ICameraInterface
    {
        Viewport viewPort;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        Vector3 cameraPosition;
        Quaternion cameraRotation;
        MouseState originalMouseState;

        public QuatCamGC(Game game) : base(game)
        {
            Game.Services.AddService(typeof(ICameraInterface), this);
        }

        public override void Initialize()
        {
            viewPort = Game.GraphicsDevice.Viewport;            

            float viewAngle = MathHelper.PiOver4;
            float nearPlane = 0.5f;
            float farPlane = 1000.0f;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, viewPort.AspectRatio, nearPlane, farPlane);

            cameraPosition = new Vector3(0, 0, 20);
            cameraRotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 0);
            UpdateViewMatrix();

#if XBOX
#else
            Mouse.SetPosition(viewPort.Width / 2, viewPort.Height / 2);
            originalMouseState = Mouse.GetState();
#endif
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Game.Exit();

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

#if XBOX        
            float rotationSpeed = 0.1f;
            float xDifference = - rotationSpeed * gamePadState.ThumbSticks.Left.X * 5.0f;
            float yDifference = rotationSpeed * gamePadState.ThumbSticks.Left.Y * 5.0f;
            float leftrightRot = rotationSpeed * xDifference;
            float updownRot = rotationSpeed * yDifference;

            Quaternion additionalRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), updownRot) * Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRot);
            cameraRotation = cameraRotation * additionalRotation;

            UpdateViewMatrix();

            float moveUp = gamePadState.Triggers.Right - gamePadState.Triggers.Left;
            AddToCameraPosition(new Vector3(gamePadState.ThumbSticks.Right.X, moveUp, -gamePadState.ThumbSticks.Right.Y));
#else
            if (currentMouseState != originalMouseState)
            {
                float rotationSpeed = 0.005f;
                float xDifference = originalMouseState.X - currentMouseState.X;
                float yDifference = originalMouseState.Y - currentMouseState.Y;
                float leftrightRot = rotationSpeed * xDifference;
                float updownRot = rotationSpeed * yDifference;
                Mouse.SetPosition(viewPort.Width / 2, viewPort.Height / 2);

                Quaternion additionalRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), updownRot) * Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRot);
                cameraRotation = cameraRotation * additionalRotation;

                UpdateViewMatrix();
            }

            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))      //Forward
                AddToCameraPosition(new Vector3(0, 0, -1));
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))    //Backward
                AddToCameraPosition(new Vector3(0, 0, 1));
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))   //Right
                AddToCameraPosition(new Vector3(1, 0, 0));
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))    //Left
                AddToCameraPosition(new Vector3(-1, 0, 0));
            if (keyState.IsKeyDown(Keys.Q))                                     //Up
                AddToCameraPosition(new Vector3(0, 1, 0));
            if (keyState.IsKeyDown(Keys.Z))                                     //Down
                AddToCameraPosition(new Vector3(0, -1, 0));
#endif
        }

        public void AddToCameraPosition(Vector3 vectorToAdd)
        {
            float moveSpeed = 0.5f;
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        public void UpdateViewMatrix()
        {
            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public Matrix ViewMatrix { get{return viewMatrix; }}

        public Vector3 Position { get { return cameraPosition; } }

        public Vector3 Forward
        {
            get
            {
                Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
                Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
                return cameraRotatedTarget;
            }
        }

        public Vector3 UpVector
        {
            get
            {
                Vector3 cameraOriginalUp = new Vector3(0, 1, 0);
                Vector3 cameraRotatedUp = Vector3.Transform(cameraOriginalUp, cameraRotation);
                return cameraRotatedUp;
            }
        }
    }
}
