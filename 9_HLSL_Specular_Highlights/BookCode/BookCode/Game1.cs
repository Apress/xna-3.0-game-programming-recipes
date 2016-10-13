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
        QuakeCamera fpsCam;

        VertexPositionNormalTexture[] vertices;
        VertexDeclaration myVertexDeclaration;
        Effect effect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(6.0f, 4.0f, 4.0f), 0, -MathHelper.Pi / 30.0f);
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            effect = Content.Load<Effect>("specular");
            InitVertices();            
        }

        private void InitVertices()
        {
            vertices = new VertexPositionNormalTexture[8];

            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector2(0, 1));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, 0, -30), new Vector3(0, 1, 0), new Vector2(0, 0));

            vertices[2] = new VertexPositionNormalTexture(new Vector3(10, 0, 0), new Vector3(0, 1, 0), new Vector2(1, 1));
            vertices[3] = new VertexPositionNormalTexture(new Vector3(10, 0, -30), new Vector3(0, 1, 0), new Vector2(1, 0));

            vertices[4] = new VertexPositionNormalTexture(new Vector3(10, 0, 0), new Vector3(-1, 0, 0), new Vector2(0, 1));
            vertices[5] = new VertexPositionNormalTexture(new Vector3(10, 0, -30), new Vector3(-1, 0, 0), new Vector2(0, 0));

            vertices[6] = new VertexPositionNormalTexture(new Vector3(10, 10, 0), new Vector3(-1, 0, 0), new Vector2(1, 1));
            vertices[7] = new VertexPositionNormalTexture(new Vector3(10, 10, -30), new Vector3(-1, 0, 0), new Vector2(1, 0));

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);
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
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            float variation = (float)Math.Sin(time)*8.0f;

            //draw triangles
            device.RenderState.CullMode = CullMode.None;

            effect.CurrentTechnique = effect.Techniques["SpecularLighting"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            effect.Parameters["xAmbient"].SetValue(0.2f);
            effect.Parameters["xLightStrength"].SetValue(0.7f);
            effect.Parameters["xLightPosition"].SetValue(new Vector3(5.0f, 2.0f, -15.0f+variation));
            effect.Parameters["xCameraPos"].SetValue(fpsCam.Position);
            effect.Parameters["xSpecularPower"].SetValue(128.0f);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip, vertices, 0, 6);
                pass.End();
            }
            effect.End();
            base.Draw(gameTime);
        }
    }
}
