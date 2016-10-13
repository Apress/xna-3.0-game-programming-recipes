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
        Texture2D blueTexture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(-2, 1, 2.0f), -MathHelper.PiOver4, -MathHelper.Pi / 8.0f);
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            blueTexture = Content.Load<Texture2D>("blue");
            InitVertices();            
        }

        private void InitVertices()
        {
            vertices = new VertexPositionNormalTexture[6];

            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, -1, 0), new Vector3(-1, 0, 0), new Vector2(0, 1));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector2(0.5f, 0));
            vertices[2] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector2(0.5f, 1));

            vertices[3] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector2(0.5f, 1));
            vertices[4] = new VertexPositionNormalTexture(new Vector3(0, 0, -1), new Vector3(0, 1, 0), new Vector2(0.5f, 1));
            vertices[5] = new VertexPositionNormalTexture(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector2(1, 1));

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
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1, 0);

            //draw triangles
            device.RenderState.CullMode = CullMode.None;

            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.Texture = blueTexture;
            basicEffect.TextureEnabled = true;

            basicEffect.LightingEnabled = true;
            Vector3 lightDirection = new Vector3(10, -2, 0);
            lightDirection.Normalize();
            basicEffect.DirectionalLight0.Direction = lightDirection;
            basicEffect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
            basicEffect.DirectionalLight0.Enabled = true;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, 2);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
