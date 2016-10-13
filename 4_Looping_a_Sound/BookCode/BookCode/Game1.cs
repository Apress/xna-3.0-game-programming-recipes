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

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            audioEngine = new AudioEngine("Content/Audio/MyXACTproject.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Audio/myWaveBank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Audio/mySoundBank.xsb");

            cue1 = soundBank.GetCue("audio1");
            cue1.Play();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();            
            
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Space) || (gamePadState.Buttons.B == ButtonState.Pressed))
            {
                if ((cue1 == null) || (cue1.IsStopped))
                {
                    cue1 = soundBank.GetCue("audio1");
                    cue1.Play();
                }
            }
            if (keyState.IsKeyDown(Keys.Enter) || (gamePadState.Buttons.A == ButtonState.Pressed))
                if (cue1 != null)
                    cue1.Stop(AudioStopOptions.Immediate);

            audioEngine.Update();

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            base.Draw(gameTime);
        }
    }
}
