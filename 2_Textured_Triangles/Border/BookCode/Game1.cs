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
        CoordCross cCross;

        private Texture2D myTexture;
        private VertexPositionTexture[] vertices;
        private VertexDeclaration myVertexDeclaration;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport);            
            base.Initialize();
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            InitVertices();
            cCross = new CoordCross(device);
            myTexture = Content.Load<Texture2D>("XNAtexture");
        }

        private void InitVertices()
        {
            vertices = new VertexPositionTexture[3];
            int i = 0;

            vertices[i++] = new VertexPositionTexture(new Vector3(-3, -3, -1), new Vector2(-0.5f, 1.5f));
            vertices[i++] = new VertexPositionTexture(new Vector3(0, 5, -1), new Vector2(0.5f, -1.5f));
            vertices[i++] = new VertexPositionTexture(new Vector3(3, -3, -1), new Vector2(1.5f, 1.5f));

            myVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
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

            cCross.Draw(fpsCam.ViewMatrix, fpsCam.ProjectionMatrix);

            //draw triangles
            device.RenderState.CullMode = CullMode.None;
            basicEffect.World = Matrix.Identity;
            basicEffect.View = fpsCam.ViewMatrix;
            basicEffect.Projection = fpsCam.ProjectionMatrix;
            basicEffect.Texture = myTexture;
            basicEffect.TextureEnabled = true;

            device.SamplerStates[0].BorderColor = Color.LightSeaGreen;
            device.SamplerStates[0].AddressU = TextureAddressMode.Border;
            device.SamplerStates[0].AddressV = TextureAddressMode.Border;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, 1);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
