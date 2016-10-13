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
    public struct SpotLight
    {
        public Vector3 Position;
        public float Strength;
        public Vector3 Direction;
        public float ConeAngle;
        public float ConeDecay;
        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;
    }

    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        const int NumberOfLights = 6;

        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        QuakeCamera fpsCam;

        VertexPositionNormalTexture[] wallVertices;
        VertexDeclaration wallVertexDeclaration;

        Effect effect1Scene;
        Effect effect2Lights;
        Effect effect3Final;
        Effect effectShadowMap;

        VertexPositionTexture[] fsVertices;
        VertexDeclaration fsVertexDeclaration;

        VertexDeclaration towerVertexDeclaration;
        VertPosTexNormTan[] towerVertices;
        int[] towerIndices;

        RenderTarget2D colorTarget;
        RenderTarget2D normalTarget;
        RenderTarget2D depthTarget;
        RenderTarget2D shadingTarget;
        RenderTarget2D shadowTarget;

        Texture2D colorMap;
        Texture2D normalMap;
        Texture2D depthMap;
        Texture2D shadingMap;
        Texture2D shadowMap;
        Texture2D blackImage;

        Texture2D wallTexture;

        SpotLight[] spotLights;        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(0.0f, 40.0f, 60.0f), 0, -MathHelper.Pi / 6.0f);
            spotLights = new SpotLight[NumberOfLights];            

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            wallTexture = Content.Load<Texture2D>("wall");

            effect1Scene = Content.Load<Effect>("Deferred1Scene");
            effect2Lights = Content.Load<Effect>("Deferred2Lights");
            effect3Final = Content.Load<Effect>("Deferred3Final");
            effectShadowMap = Content.Load<Effect>("ShadowMap");            

            PresentationParameters pp = device.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            colorTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Color);
            normalTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Color);
            depthTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Single);
            shadingTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Color);            
            shadowTarget = new RenderTarget2D(device, width, height, 1, SurfaceFormat.Single);
            blackImage = new Texture2D(device, width, height, 1, TextureUsage.None, SurfaceFormat.Color);

            InitSceneVertices();
            InitFullscreenVertices();
            towerVertexDeclaration = new VertexDeclaration(device, VertPosTexNormTan.VertexElements);
            towerVertices = InitTowerVertices();
            towerIndices = InitTowerIndices(towerVertices);
            towerVertices = GenerateNormalsForTriangleList(towerVertices, towerIndices);
        }

        private void InitFullscreenVertices()
        {
            fsVertices = new VertexPositionTexture[4];
            int i = 0;
            fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 0));
            fsVertices[i++] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 0));
            fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 1));
            fsVertices[i++] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 1));

            fsVertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionTexture.VertexElements);
        }        

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            //process user input
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            fpsCam.Update(mouseState, keyState, gamePadState);

            //update lights
            float coneAngle = MathHelper.PiOver4;
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            float sine = (float)Math.Sin(time);
            for (int i = 0; i < NumberOfLights; i++)
            {                
                Matrix yRot = Matrix.CreateRotationY(time / 2.0f + (float)i * MathHelper.Pi * 2.0f / (float)NumberOfLights);
                Matrix xRot = Matrix.CreateRotationX(-MathHelper.PiOver2+sine+0.4f);
                Matrix fullRot = xRot * yRot;                

                Vector3 lightPosition = new Vector3(0.0f, 1.5f, 0.0f) + 5.0f * Vector3.Transform(Vector3.Forward, yRot);
                Vector3 lightDirection = Vector3.Transform(Vector3.Forward, fullRot);
                Vector3 lightUp = Vector3.Transform(Vector3.Up, fullRot); ;

                spotLights[i].Position = lightPosition;
                spotLights[i].Strength = 0.4f;
                spotLights[i].Direction = lightDirection;
                spotLights[i].ConeAngle = (float)Math.Cos(coneAngle);
                spotLights[i].ConeDecay = 1.5f;
                spotLights[i].ViewMatrix = Matrix.CreateLookAt(lightPosition, lightPosition + lightDirection,lightUp);
                float viewAngle = (float)Math.Acos(spotLights[i].ConeAngle);
                spotLights[i].ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(coneAngle * 2.0f, 1.0f, 0.5f, 1000.0f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //render color, normal and depth into 3 render targets
            RenderSceneTo3RenderTargets();
            
            //Add lighting contribution of each light onto shadingMap
            shadingMap = GenerateShadingMap();
            
            //Combine base color map and shading map
            CombineColorAndShading();            

            base.Draw(gameTime);
        }

        private void RenderSceneTo3RenderTargets()
        {
            //bind render targets to outputs of pixel shaders
            device.SetRenderTarget(0, colorTarget);
            device.SetRenderTarget(1, normalTarget);
            device.SetRenderTarget(2, depthTarget);

            //clear all render targets
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            //render the scene using custom effect that writes to all render targets simultaneously
            effect1Scene.CurrentTechnique = effect1Scene.Techniques["MultipleTargets"];
            effect1Scene.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            effect1Scene.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            RenderScene(effect1Scene);

            //de-activate render targets to resolve them
            device.SetRenderTarget(0, null);
            device.SetRenderTarget(1, null);
            device.SetRenderTarget(2, null);

            //copy contents of render targets into texture
            colorMap = colorTarget.GetTexture();
            normalMap = normalTarget.GetTexture();
            depthMap = depthTarget.GetTexture();
        }        

        private Texture2D GenerateShadingMap()
        {
            shadingMap = blackImage;

            for (int i = 0; i < NumberOfLights; i++)
            {
                RenderShadowMap(spotLights[i]);
                AddLight(spotLights[i]);
            }
            
            return shadingTarget.GetTexture();
        }

        private void RenderShadowMap(SpotLight spotLight)
        {
            device.SetRenderTarget(0, shadowTarget);

            effectShadowMap.CurrentTechnique = effectShadowMap.Techniques["ShadowMap"];
            effectShadowMap.Parameters["xView"].SetValue(spotLight.ViewMatrix);
            effectShadowMap.Parameters["xProjection"].SetValue(spotLight.ProjectionMatrix);
            RenderScene(effectShadowMap);            

            device.SetRenderTarget(0, null);
            shadowMap = shadowTarget.GetTexture();
        }

        private void AddLight(SpotLight spotLight)
        {
            device.SetRenderTarget(0, shadingTarget);

            effect2Lights.CurrentTechnique = effect2Lights.Techniques["DeferredSpotLight"];
            effect2Lights.Parameters["xPreviousShadingContents"].SetValue(shadingMap);
            effect2Lights.Parameters["xNormalMap"].SetValue(normalMap);
            effect2Lights.Parameters["xDepthMap"].SetValue(depthMap);
            effect2Lights.Parameters["xShadowMap"].SetValue(shadowMap);

            effect2Lights.Parameters["xLightPosition"].SetValue(spotLight.Position);
            effect2Lights.Parameters["xLightStrength"].SetValue(spotLight.Strength);
            effect2Lights.Parameters["xConeDirection"].SetValue(spotLight.Direction);
            effect2Lights.Parameters["xConeAngle"].SetValue(spotLight.ConeAngle);
            effect2Lights.Parameters["xConeDecay"].SetValue(spotLight.ConeDecay);

            Matrix viewProjInv = Matrix.Invert(fpsCam.ViewMatrix * fpsCam.ProjectionMatrix);
            effect2Lights.Parameters["xViewProjectionInv"].SetValue(viewProjInv);
            effect2Lights.Parameters["xLightViewProjection"].SetValue(spotLight.ViewMatrix * spotLight.ProjectionMatrix);

            effect2Lights.Begin();
            foreach (EffectPass pass in effect2Lights.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = fsVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, fsVertices, 0, 2);
                pass.End();
            }
            effect2Lights.End();

            device.SetRenderTarget(0, null);
            shadingMap = shadingTarget.GetTexture();
        }        
        
        private void CombineColorAndShading()
        {
            effect3Final.CurrentTechnique = effect3Final.Techniques["CombineColorAndShading"];
            effect3Final.Parameters["xColorMap"].SetValue(colorMap);
            effect3Final.Parameters["xShadingMap"].SetValue(shadingMap);
            effect3Final.Parameters["xAmbient"].SetValue(0.3f);

            effect3Final.Begin();
            foreach (EffectPass pass in effect3Final.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = fsVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, fsVertices, 0, 2);
                pass.End();
            }
            effect3Final.End();
        }

        private void RenderScene(Effect effect)
        {
            //Render room
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xTexture"].SetValue(wallTexture);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = wallVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, wallVertices, 0, 14);
                pass.End();
            }
            effect.End();

            //Render 9 towers
            for (int i = 0; i < 9; i++)
            {
                Vector3 towerPosition = Vector3.Transform(20.0f * Vector3.Right, Matrix.CreateRotationY(MathHelper.Pi / 8.0f * (float)i));
                RenderTower(effect, Matrix.CreateTranslation(towerPosition));
            }
        }

        private void RenderTower(Effect effect, Matrix worldMatrix)
        {
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xTexture"].SetValue(wallTexture);
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = towerVertexDeclaration;
                device.DrawUserPrimitives<VertPosTexNormTan>(PrimitiveType.TriangleStrip, towerVertices, 0, towerVertices.Length - 2);
                pass.End();
            }
            effect.End();
        }
    }
}
