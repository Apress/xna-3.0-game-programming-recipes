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

        VertexPositionTexture[] ppVertices;
        VertexDeclaration ppVertexDeclaration;
        RenderTarget2D targetRenderedTo;
        ResolveTexture2D resolveTexture;
        Effect postProcessingEffect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(graphics.GraphicsDevice.Viewport);
            InitPostProcessingVertices();
            base.Initialize();            
        }

        private void InitPostProcessingVertices()
        {
            ppVertices = new VertexPositionTexture[4];
            int i = 0;
            ppVertices[i++] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 0));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 0));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 1));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 1));

            ppVertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionTexture.VertexElements);
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);

            myModel = Content.Load<Model>("Ship");
            modelTransforms = new Matrix[myModel.Bones.Count];

            PresentationParameters pp = GraphicsDevice.PresentationParameters;            
            targetRenderedTo = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            resolveTexture = new ResolveTexture2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            postProcessingEffect = Content.Load<Effect>("postprocessing");
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

            PostProcess();

            base.Draw(gameTime);
        }

        public void PostProcess()
        {        
            device.ResolveBackBuffer(resolveTexture, 0);
            Texture2D textureRenderedTo = resolveTexture;
            
            postProcessingEffect.CurrentTechnique = postProcessingEffect.Techniques["Invert"];
            postProcessingEffect.Begin();
            postProcessingEffect.Parameters["textureToSampleFrom"].SetValue(textureRenderedTo);
            foreach (EffectPass pass in postProcessingEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = ppVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, ppVertices, 0, 2);
                pass.End();
            }
            postProcessingEffect.End();            
        }
    }


}