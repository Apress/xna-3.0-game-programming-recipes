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
    public struct VertexExplosion
    {
        public Vector3 Position;
        public Vector4 TexCoord;
        public Vector4 AdditionalInfo;
        public VertexExplosion(Vector3 Position, Vector4 TexCoord, Vector4 AdditionalInfo)
        {
            this.Position = Position;
            this.TexCoord = TexCoord;
            this.AdditionalInfo = AdditionalInfo;
        }
        public static readonly VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, 12, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0, 28, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1),
            };
        public static readonly int SizeInBytes = sizeof(float) * (3 + 4 + 4);
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        BasicEffect basicEffect;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        CoordCross cCross;
        QuatCam quatCam;

        Texture2D myTexture;
        VertexExplosion[] explosionVertices;
        VertexDeclaration myVertexDeclaration;
        Effect expEffect;
        float time = 0;
        Random rand;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            quatCam = new QuatCam(GraphicsDevice.Viewport);
            rand = new Random();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            basicEffect = new BasicEffect(device, null);
            cCross = new CoordCross(device);

            myTexture = Content.Load<Texture2D>("explosion");
            expEffect = Content.Load<Effect>("explosionEffect");

            myVertexDeclaration = new VertexDeclaration(device, VertexExplosion.VertexElements);

            //only used to render one line of text!
            spriteBatch = new SpriteBatch(device);
            spriteFont = Content.Load<SpriteFont>("arial");
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

            quatCam.Update(mouseState, keyState, gamePadState);

            if ((keyState.IsKeyDown(Keys.Space)) || (gamePadState.Buttons.A == ButtonState.Pressed))
                CreateExplosionVertices((float)gameTime.TotalGameTime.TotalMilliseconds);

            base.Update(gameTime);
        }

        private void CreateExplosionVertices(float time)
        {
            int particles = 80;
            explosionVertices = new VertexExplosion[particles * 6];

            int i = 0;
            for (int partnr = 0; partnr < particles; partnr++)
            {
                Vector3 startingPos = new Vector3(5, 0, 0);

                float r1 = (float)rand.NextDouble() - 0.5f;
                float r2 = (float)rand.NextDouble() - 0.5f;
                float r3 = (float)rand.NextDouble() - 0.5f;
                Vector3 moveDirection = new Vector3(r1, r2, r3);
                moveDirection.Normalize();

                float r4 = (float)rand.NextDouble();
                r4 = r4 / 4.0f * 3.0f + 0.25f;

                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(1, 1, time, 1000), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(0, 0, time, 1000), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(1, 0, time, 1000), new Vector4(moveDirection, r4));

                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(1, 1, time, 1000), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(0, 1, time, 1000), new Vector4(moveDirection, r4));
                explosionVertices[i++] = new VertexExplosion(startingPos, new Vector4(0, 0, time, 1000), new Vector4(moveDirection, r4));
            }
        }
        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            cCross.Draw(quatCam.ViewMatrix, quatCam.ProjectionMatrix);

            if (explosionVertices != null)
            {
                //draw billboards
                expEffect.CurrentTechnique = expEffect.Techniques["Explosion"];
                expEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
                expEffect.Parameters["xProjection"].SetValue(quatCam.ProjectionMatrix);
                expEffect.Parameters["xView"].SetValue(quatCam.ViewMatrix);

                expEffect.Parameters["xCamPos"].SetValue(quatCam.Position);
                expEffect.Parameters["xExplosionTexture"].SetValue(myTexture);
                expEffect.Parameters["xCamUp"].SetValue(quatCam.UpVector);
                expEffect.Parameters["xTime"].SetValue((float)gameTime.TotalGameTime.TotalMilliseconds);

                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.One;
                device.RenderState.DepthBufferWriteEnable = false;
            
                expEffect.Begin();
                foreach (EffectPass pass in expEffect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = myVertexDeclaration;
                    device.DrawUserPrimitives<VertexExplosion>(PrimitiveType.TriangleList, explosionVertices, 0, explosionVertices.Length / 3);
                    pass.End();
                }
                expEffect.End();

                device.RenderState.DepthBufferWriteEnable = true;                
            }

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            spriteBatch.DrawString(spriteFont, "Hit Space to initiate an explosion!", new Vector2(20, 20), Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
