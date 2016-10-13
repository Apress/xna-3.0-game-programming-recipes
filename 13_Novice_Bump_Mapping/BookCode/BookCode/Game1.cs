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
        QuakeCamera fpsCam;

        private VertexPositionTexture[] vertices;
        private VertexDeclaration myVertexDeclaration;
        private Texture2D myTexture;
        private Texture2D myBumpMap;
        private Effect effect;
        private Arrow arrow;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(0, 5, 12), 0, -MathHelper.Pi / 6.0f);            
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;            
            vertices = InitVertices();
            arrow = new Arrow(device);
            myTexture = Content.Load<Texture2D>("wall");
            myBumpMap = Content.Load<Texture2D>("wall_normal_map");
            effect = Content.Load<Effect>("novicebumpmapping");
        }

        private VertexPositionTexture[] InitVertices()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[6];

            vertices[0] = new VertexPositionTexture(new Vector3(-5, 0, 10), new Vector2(0, 2));
            vertices[1] = new VertexPositionTexture(new Vector3(-5, 0, -10), new Vector2(0, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(5, 0, 10), new Vector2(1, 2));

            vertices[3] = new VertexPositionTexture(new Vector3(-5, 0, -10), new Vector2(0, 0));
            vertices[4] = new VertexPositionTexture(new Vector3(5, 0, -10), new Vector2(1, 0));
            vertices[5] = new VertexPositionTexture(new Vector3(5, 0, 10), new Vector2(1, 2));

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

            return vertices;
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

            //calculate light rotatation matrix
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            Matrix lightRotation = Matrix.CreateRotationZ(2.0f * (float)Math.Sin((time / 2.0f)));
            Vector3 lightDirection = Vector3.Transform(new Vector3(0, -1, 0), lightRotation);
            arrow.Draw(lightRotation, fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            //set effect parameters
            effect.CurrentTechnique = effect.Techniques["SimpleBumpMapping"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            effect.Parameters["xTexture"].SetValue(myTexture);
            effect.Parameters["xBumpMap"].SetValue(myBumpMap);
            effect.Parameters["xLightDirection"].SetValue(lightDirection);

            //render 2 triangles
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, 2);
                pass.End();
            }
            effect.End();

            base.Draw(gameTime);
        }
    }
}
