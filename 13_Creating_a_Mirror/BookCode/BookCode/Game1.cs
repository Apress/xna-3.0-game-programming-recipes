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
        CoordCross cCross;
        QuakeCamera fpsCam;

        RenderTarget2D renderTarget;
        Texture2D mirrorTexture;
        VertexPositionTexture[] mirrorVertices;
        VertexDeclaration myVertexDeclaration;
        Matrix mirrorViewMatrix;

        Plane mirrorPlane;        
        Effect mirrorEffect;
        Plane clipPlane;

        Model myModel;
        Matrix[] modelTransforms;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(5,0,20), 0, 0);
            InitMirror();            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);

            PresentationParameters pp = device.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            renderTarget = new RenderTarget2D(device, width, height, 1, device.DisplayMode.Format);
            mirrorEffect = Content.Load<Effect>("mirrorEffect");

            myModel = Content.Load<Model>("dude");
            modelTransforms = new Matrix[myModel.Bones.Count];
        }

        private void InitMirror()
        {
            mirrorVertices = new VertexPositionTexture[4];
            int i = 0;

            Vector3 p0 = new Vector3(-3, 0, 0);
            Vector3 p1 = new Vector3(-3, 6, 0);
            Vector3 p2 = new Vector3(6, 0, 0);

            Vector3 p3 = p1 + p2 - p0;

            mirrorVertices[i++] = new VertexPositionTexture(p0, new Vector2(0, 0));
            mirrorVertices[i++] = new VertexPositionTexture(p1, new Vector2(0, 0));
            mirrorVertices[i++] = new VertexPositionTexture(p2, new Vector2(0, 0));
            mirrorVertices[i++] = new VertexPositionTexture(p3, new Vector2(0, 0));

            mirrorPlane = new Plane(p0, p1, p2);

            myVertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionTexture.VertexElements);
        }

        private void UpdateMirrorViewMatrix()
        {
            Vector3 mirrorCamPosition = MirrorVector3(mirrorPlane, fpsCam.Position);
            Vector3 mirrorTargetPosition = MirrorVector3(mirrorPlane, fpsCam.TargetPosition);

            Vector3 camUpPosition = fpsCam.Position + fpsCam.UpVector;
            Vector3 mirrorCamUpPosition = MirrorVector3(mirrorPlane, camUpPosition);
            Vector3 mirrorUpVector = mirrorCamUpPosition - mirrorCamPosition;

            mirrorViewMatrix = Matrix.CreateLookAt(mirrorCamPosition, mirrorTargetPosition, mirrorUpVector);
        }

        private Vector3 MirrorVector3(Plane mirrorPlane, Vector3 originalV3)
        {
            float distV3ToPlane = mirrorPlane.DotCoordinate(originalV3);
            Vector3 mirroredV3 = originalV3 - 2 * distV3ToPlane * mirrorPlane.Normal;
            return mirroredV3;
        }

        private void UpdateClipPlane()
        {
            Matrix camMatrix = mirrorViewMatrix * fpsCam.ProjectionMatrix;
            Matrix invCamMatrix = Matrix.Invert(camMatrix);
            invCamMatrix = Matrix.Transpose(invCamMatrix);

            Vector4 mirrorPlaneCoeffs = new Vector4(mirrorPlane.Normal, mirrorPlane.D);
            Vector4 clipPlaneCoeffs = Vector4.Transform(-mirrorPlaneCoeffs, invCamMatrix);
            clipPlane = new Plane(clipPlaneCoeffs);
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

            UpdateMirrorViewMatrix();
            UpdateClipPlane();

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            //render scene as seen by mirror into rendertarget
            device.SetRenderTarget(0, renderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);
            device.ClipPlanes[0].Plane = clipPlane;
            device.ClipPlanes[0].IsEnabled = true;
            RenderScene(mirrorViewMatrix, fpsCam.ProjectionMatrix);
            device.ClipPlanes[0].IsEnabled = false;

            //deactivate custom rendertarget, and save its contents into a texture
            device.SetRenderTarget(0, null);
            mirrorTexture = renderTarget.GetTexture();

            //render scene + mirror as seen by user to screen            
            graphics.GraphicsDevice.Clear(Color.Tomato);
            RenderScene(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);
            RenderMirror();

            base.Draw(gameTime);
        }

        private void RenderScene(Matrix viewMatrix, Matrix projectionMatrix)
        {
            //draw coord translated coord cross
            basicEffect.World = Matrix.CreateTranslation(2, 5, 6);
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.TextureEnabled = false;
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                cCross.DrawUsingPresetEffect();
                pass.End();
            }
            basicEffect.End();

            //draw model in front of mirror
            Matrix worldMatrix = Matrix.CreateScale(0.05f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, 0, 5);
            myModel.CopyAbsoluteBoneTransformsTo(modelTransforms);
            foreach (ModelMesh mesh in myModel.Meshes)
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

            //draw model behind mirror (this model should not be seen in the mirror!)
            worldMatrix = Matrix.CreateScale(0.05f) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(0, 2, -10);
            myModel.CopyAbsoluteBoneTransformsTo(modelTransforms);
            foreach (ModelMesh mesh in myModel.Meshes)
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
        }

        private void RenderMirror()
        {
            mirrorEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            mirrorEffect.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            mirrorEffect.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            mirrorEffect.Parameters["xMirrorView"].SetValue(mirrorViewMatrix);
            mirrorEffect.Parameters["xMirrorTexture"].SetValue(mirrorTexture);

            mirrorEffect.Begin();
            foreach (EffectPass pass in mirrorEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, mirrorVertices, 0, 2);
                pass.End();
            }
            mirrorEffect.End();
        }
    }
}
