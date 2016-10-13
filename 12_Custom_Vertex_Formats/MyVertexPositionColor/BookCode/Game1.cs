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
    public struct MyVertexPositionColor
    {
        public Vector3 Position;
        public Color Color;
        public MyVertexPositionColor(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public static readonly VertexElement[] VertexElements =
            {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, sizeof(float)*3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0),
            };
        public static readonly int SizeInBytes = sizeof(float) * (3 + 1);
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        BasicEffect basicEffect;
        QuakeCamera fpsCam;
        CoordCross cCross;
        VertexBuffer vertBuffer;
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
            InitVertices();            
        }

        private void InitVertices()
        {
            MyVertexPositionColor[] vertices = new MyVertexPositionColor[3];
            int i = 0;

            vertices[i++] = new MyVertexPositionColor(new Vector3(1, 1, -1), Color.Red);
            vertices[i++] = new MyVertexPositionColor(new Vector3(3, 5, -1), Color.Green);
            vertices[i++] = new MyVertexPositionColor(new Vector3(5, 1, -1), Color.Blue);

            vertBuffer = new VertexBuffer(device, MyVertexPositionColor.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertBuffer.SetData<MyVertexPositionColor>(vertices, 0, vertices.Length);

            myVertexDeclaration = new VertexDeclaration(device, MyVertexPositionColor.VertexElements);
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
            basicEffect.VertexColorEnabled = true;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = myVertexDeclaration;
                device.Vertices[0].SetSource(vertBuffer, 0, MyVertexPositionColor.SizeInBytes);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                pass.End();
            }
            basicEffect.End();

            base.Draw(gameTime);
        }
    }
}
