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

        PostProcessor postProcessor;

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

            myModel = Content.Load<Model>("Ship");
            modelTransforms = new Matrix[myModel.Bones.Count];

            Effect ppEffect = Content.Load<Effect>("postprocessing");
            postProcessor = new PostProcessor(device, ppEffect);
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
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;

            device.Clear(ClearOptions.Target|ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            //draw model
            Matrix worldMatrix = Matrix.CreateScale(0.003f) * Matrix.CreateTranslation(0, 0, 0);       
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

            //draw coordcross
            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            base.Draw(gameTime);

            List<string> ppEffectsList = new List<string>();
            ppEffectsList.Add("Invert");
            ppEffectsList.Add("TimeChange");
            postProcessor.Parameters["xTime"].SetValue(time);
            postProcessor.PostProcess(ppEffectsList);            
        }
    }
}