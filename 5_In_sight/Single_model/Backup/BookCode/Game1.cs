
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace BookCode
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;
        GraphicsDevice device;
        QuakeCamera fpsCam;
        CoordCross cCross;

        Model myModel;
        Matrix[] modelTransforms;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(graphics.GraphicsDevice.Viewport);            
            base.Initialize();            
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);

            myModel = XNAUtils.LoadModelWithBoundingSphere(ref modelTransforms, "Ship", Content);
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
            Matrix worldMatrix = Matrix.CreateScale(0.002f) * Matrix.CreateTranslation(5, 0, 0);                    

            BoundingFrustum cameraSight = new BoundingFrustum(fpsCam.ViewMatrix * fpsCam.ProjectionMatrix);
            BoundingSphere origSphere = (BoundingSphere)myModel.Tag;
            BoundingSphere transSphere = origSphere.Transform(worldMatrix);
            
            ContainmentType containmentType = cameraSight.Contains(transSphere);            

            if (containmentType != ContainmentType.Disjoint)
            {
                Window.Title = "Model inside frustum";
                graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

                //draw model
                myModel.CopyAbsoluteBoneTransformsTo(modelTransforms);
                foreach (ModelMesh mesh in myModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                        effect.View = fpsCam.ViewMatrix;
                        effect.Projection = fpsCam.ProjectionMatrix;
                    }
                    mesh.Draw();
                }
            }
            else
            {
                Window.Title = "Model outside frustum";
                graphics.GraphicsDevice.Clear(Color.Red);
            }            

            //draw coordcross
            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);            

            base.Draw(gameTime);
        }
    }
}