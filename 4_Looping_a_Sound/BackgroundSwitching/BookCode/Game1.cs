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

        string[] bgCueNames;
        Cue currentCue;
        int currentCueNr = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            InitSounds();

            base.Initialize();
        }

        private void InitSounds()
        {
            audioEngine = new AudioEngine("Content/Audio/MyXACTproject.xgs");
            waveBank = new WaveBank(audioEngine, "Content/Audio/myWaveBank.xwb");
            soundBank = new SoundBank(audioEngine, "Content/Audio/mySoundBank.xsb");

            bgCueNames = new string[4];
            bgCueNames[0] = "bgAudio1";
            bgCueNames[1] = "bgAudio2";
            bgCueNames[2] = "bgAudio3";
            bgCueNames[3] = "bgAudio4";

            PlayBGCue(0);
        }

        private void PlayBGCue(int cueNr)
        {
            currentCue = soundBank.GetCue(bgCueNames[cueNr]);
            currentCue.Play();
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

            UpdateSounds();

            base.Update(gameTime);
        }

        private void UpdateSounds()
        {
            if (currentCue.IsStopped)
            {
                if (++currentCueNr == bgCueNames.Length)
                    currentCueNr = 0;

                PlayBGCue(currentCueNr);
            }

            audioEngine.Update();
        }


        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1, 0);

            base.Draw(gameTime);
        }
    }
}
