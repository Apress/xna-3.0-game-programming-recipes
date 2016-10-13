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
        Model spacecraftModel;
        Matrix[] modelTransforms;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        Vector3 spacecraftPosition;
        Quaternion spacecraftRotation;

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

            spacecraftPosition = new Vector3(-1, 1, 10);
            spacecraftRotation = Quaternion.Identity;
            UpdateViewMatrix();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            cCross = new CoordCross(device);
            spacecraftModel = Content.Load<Model>("Ship");
            modelTransforms = new Matrix[spacecraftModel.Bones.Count];
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

            leftrightRotation -= gamePadState.ThumbSticks.Left.X / 50.0f;
            updownRotation += gamePadState.ThumbSticks.Left.Y / 50.0f;

            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.Up))
                updownRotation = 0.05f;
            if (keys.IsKeyDown(Keys.Down))
                updownRotation = -0.05f;
            if (keys.IsKeyDown(Keys.Right))
                leftrightRotation = -0.05f;
            if (keys.IsKeyDown(Keys.Left))
                leftrightRotation = 0.05f;

            leftrightRotation -= gamePadState.ThumbSticks.Left.X/50.0f;
            updownRotation += gamePadState.ThumbSticks.Left.Y/50.0f;

            Quaternion additionalRotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), updownRotation) * Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftrightRotation);
            spacecraftRotation = spacecraftRotation * additionalRotation;

            AddToSpacecraftPosition(new Vector3(0, 0, -1));

            base.Update(gameTime);
        }

        private void AddToSpacecraftPosition(Vector3 vectorToAdd)
        {
            float moveSpeed = 0.05f;
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, spacecraftRotation);
            spacecraftPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Vector3 cameraOriginalPosition = new Vector3(0, 0, 1);
            Vector3 cameraRotatedPosition = Vector3.Transform(cameraOriginalPosition, spacecraftRotation);
            Vector3 cameraFinalPosition = spacecraftPosition + cameraRotatedPosition;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, spacecraftRotation);

            viewMatrix = Matrix.CreateLookAt(cameraFinalPosition, spacecraftPosition, cameraRotatedUpVector);
        }

    
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            //draw coordcross
            cCross.Draw(viewMatrix, projectionMatrix);

            //draw model
            Matrix worldMatrix = Matrix.CreateScale(1.0f/5000.0f)*Matrix.CreateFromQuaternion(spacecraftRotation)*Matrix.CreateTranslation(spacecraftPosition);
            spacecraftModel.CopyAbsoluteBoneTransformsTo(modelTransforms);            
            foreach (ModelMesh mesh in spacecraftModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();        
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }
            
            base.Draw(gameTime);
        }
    }
}