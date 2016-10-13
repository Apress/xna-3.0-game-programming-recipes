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
    public struct MyCustomVertexFormat
    {
        public Vector3 Position;
        public Vector2 TexCoords;
        public Vector4 Extra;
        public MyCustomVertexFormat(Vector3 Position, Vector2 TexCoords, Vector4 Extra)
        {
            this.Position = Position;
            this.TexCoords = TexCoords;
            this.Extra = Extra;
        }

        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, sizeof(float)*3, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(0, sizeof(float)*(3+2), VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1),
        };
        public static readonly int SizeInBytes = sizeof(float) * (3 + 2 + 4);
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        BasicEffect basicEffect;
        QuakeCamera fpsCam;
        CoordCross cCross;
        VertexBuffer vertBuffer;
        Effect effect;
        Texture2D myTexture;
        VertexDeclaration myVertexDeclaration;

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
            cCross = new CoordCross(device);
            effect = Content.Load<Effect>("customvertexshader");
            myTexture = Content.Load<Texture2D>("XNATexture");
            InitVertices();
        }

        private void InitVertices()
        {
            MyCustomVertexFormat[] vertices = new MyCustomVertexFormat[3];
            int i = 0;

            vertices[i++] = new MyCustomVertexFormat(new Vector3(1, 1, -1), new Vector2(0, 1), new Vector4(-1.0f, 0.5f, 0.3f, 0.5f));
            vertices[i++] = new MyCustomVertexFormat(new Vector3(3, 5, -1), new Vector2(0.5f, 0), new Vector4(0.8f, -0.2f, 0.1f, -0.5f));
            vertices[i++] = new MyCustomVertexFormat(new Vector3(5, 1, -1), new Vector2(1, 1), new Vector4(2.0f, 0.6f, -1.0f, 0.7f));

            vertBuffer = new VertexBuffer(device, MyCustomVertexFormat.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertBuffer.SetData<MyCustomVertexFormat>(vertices, 0, vertices.Length);

            myVertexDeclaration = new VertexDeclaration(device, MyCustomVertexFormat.VertexElements);
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

            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            //draw triangles
            device.RenderState.CullMode = CullMode.None;
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(fpsCam.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(fpsCam.ProjectionMatrix);
            effect.Parameters["xTexture"].SetValue(myTexture);
            effect.Parameters["xTime"].SetValue(time);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.Vertices[0].SetSource(vertBuffer, 0, MyCustomVertexFormat.SizeInBytes);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                pass.End();
            }
            effect.End();

            base.Draw(gameTime);
        }
    }
}
