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

        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;
        Cue cue1;                

        Vector3 modelPos;
        Model myModel;
        Matrix[] modelTransforms;
        QuakeCamera fpsCam;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            fpsCam = new QuakeCamera(GraphicsDevice.Viewport, new Vector3(0,0,5) , 0, 0);

            audioEngine = new AudioEngine("Content/Audio/MyXACTproject.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Audio/myWaveBank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Audio/mySoundBank.xsb");

            cue1 = soundBank.GetCue("audio1");                       

            base.Initialize();
        }

        private void UpdateSoundPosition(Cue cue, Vector3 sourcePos, Vector3 camPos, Vector3 camForward, Vector3 camUp)
        {
            AudioEmitter emitter = new AudioEmitter();
            emitter.Position = sourcePos;
            
            AudioListener listener = new AudioListener();
            listener.Position = camPos;
            listener.Forward = camForward;
            listener.Up = camUp;

            SoundEffect effect;


            cue.Apply3D(listener, emitter);
        }        

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            myModel = Content.Load<Model>("tank");
            modelTransforms = new Matrix[myModel.Bones.Count];            
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
            
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds/1000.0f;
            Vector3 startingPos = new Vector3(0, 0, -10);
            Matrix rotMatrix = Matrix.CreateRotationY(time);
            modelPos = Vector3.Transform(startingPos, rotMatrix);

            UpdateSoundPosition(cue1, modelPos, fpsCam.Position, fpsCam.Forward, fpsCam.UpVector);

            if (cue1.IsPrepared)
                cue1.Play();

            audioEngine.Update();

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            //draw model
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds/1000.0f;
            Matrix worldMatrix = Matrix.CreateScale(0.005f, 0.005f, 0.005f) * Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateTranslation(0, 0, -10) * Matrix.CreateRotationY(time);
            myModel.CopyAbsoluteBoneTransformsTo(modelTransforms);
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = fpsCam.ViewMatrix;
                    effect.Projection = fpsCam.ProjectionMatrix;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
