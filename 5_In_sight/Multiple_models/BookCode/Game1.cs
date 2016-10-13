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
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            int modelsDrawn = 0;
            BoundingFrustum cameraSight = new BoundingFrustum(fpsCam.ViewMatrix * fpsCam.ProjectionMatrix);

            for (int x = 0; x < 5; x++)
            {
                for (int z = 0; z < 5; z++)
                {
                    Matrix worldMatrix = Matrix.CreateScale(0.001f) * Matrix.CreateTranslation(x*5-10, 0, z*5-10);                                
                    BoundingSphere origSphere = (BoundingSphere)myModel.Tag;
                    BoundingSphere transSphere = origSphere.Transform(worldMatrix);
                    
                    ContainmentType containmentType = cameraSight.Contains(transSphere);            

                    if (containmentType != ContainmentType.Disjoint)
                    {
                        modelsDrawn++;
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
                }
            }
            Window.Title = string.Format("Models actually drawn: {0}", modelsDrawn);

            //draw coordcross
            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);            

            base.Draw(gameTime);
        }
    }
}