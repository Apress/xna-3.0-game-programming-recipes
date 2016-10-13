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
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(-3.5f, 3, 5.3f), -MathHelper.PiOver4, 0);
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            effect = Content.Load<Effect>("pointlight");
            InitVertices();            
        }

        private void InitVertices()
        {
            vertices = new VertexPositionNormalTexture[6];
            int i = 0;

            vertices[i++] = new VertexPositionNormalTexture(new Vector3(-1, 0, 1), new Vector3(0, 1, 0), new Vector2(1, 1));
            vertices[i++] = new VertexPositionNormalTexture(new Vector3(-1, 0, -1), new Vector3(0, 1, 0), new Vector2(0, 1));
            vertices[i++] = new VertexPositionNormalTexture(new Vector3(1, 0, -1), new Vector3(0, 1, 0), new Vector2(0, 0));

            vertices[i++] = new VertexPositionNormalTexture(new Vector3(1, 0, -1), new Vector3(0, 1, 0), new Vector2(0, 0));
            vertices[i++] = new VertexPositionNormalTexture(new Vector3(1, 0, 1), new Vector3(0, 1, 0), new Vector2(1, 0));
            vertices[i++] = new VertexPositionNormalTexture(new Vector3(-1, 0, 1), new Vector3(0, 1, 0), new Vector2(1, 1));

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

            //draw triangles
            device.RenderState.CullMode = CullMode.None;

            effect.CurrentTechnique = effect.Techniques["PointLight"];
            effect.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            effect.Parameters["xLightPosition"].SetValue(new Vector3(2, 5, 0));
            effect.Parameters["xAmbient"].SetValue(0.0f);

            for (int i = 0; i < 9; i++)
            {
                Matrix world = Matrix.CreateTranslation(4, 0, 0) * Matrix.CreateRotationZ((float)i * MathHelper.PiOver2 / 8.0f);
                effect.Parameters["xWorld"].SetValue(world);
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = myVertexDeclaration;
                    device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, 2);
                    pass.End();
                }
                effect.End();
            }

            base.Draw(gameTime);
        }
    }
}
