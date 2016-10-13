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

        List<MenuWindow> menuList;
        MenuWindow activeMenu;
        KeyboardState lastKeybState;
        GamePadState lastGamePadState;

        MenuWindow startGameEasy;
        MenuWindow startGameNormal;
        MenuWindow startGameHard;
        bool menusRunning;
        PostProcessor postProcessor;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            lastKeybState = Keyboard.GetState();
            lastGamePadState = GamePad.GetState(PlayerIndex.One);
            menuList = new List<MenuWindow>();
            menusRunning = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;
            SpriteFont menuFont = Content.Load<SpriteFont>("ourfont");
            Texture2D backgroundImage = Content.Load<Texture2D>("bg");
            Texture2D bg = Content.Load<Texture2D>("bg2");
            spriteBatch = new SpriteBatch(device);

            Effect ppEffect = Content.Load<Effect>("postprocessing");
            postProcessor = new PostProcessor(device, ppEffect);

            //dummy menus
            startGameEasy = new MenuWindow(null, null, null);
            startGameNormal = new MenuWindow(null, null, null);
            startGameHard = new MenuWindow(null, null, null);

            MenuWindow menuMain = new MenuWindow(menuFont, "Main Menu", backgroundImage);
            MenuWindow menuNewGame = new MenuWindow(menuFont, "Start a New Game", bg);
            MenuWindow menuOptions = new MenuWindow(menuFont, "Options Menu", backgroundImage);
            menuList.Add(menuMain);
            menuList.Add(menuNewGame);
            menuList.Add(menuOptions);

            menuMain.AddMenuItem("New Game", menuNewGame);
            menuMain.AddMenuItem("Load Game", menuMain);
            menuMain.AddMenuItem("Options", menuOptions);
            menuMain.AddMenuItem("Exit Game", null);

            menuNewGame.AddMenuItem("Easy", startGameEasy);
            menuNewGame.AddMenuItem("Normal", startGameNormal);
            menuNewGame.AddMenuItem("Hard", startGameHard);
            menuNewGame.AddMenuItem("Back to Main menu", menuMain);

            menuOptions.AddMenuItem("Change controls", menuMain);
            menuOptions.AddMenuItem("Change graphics setting", menuMain);
            menuOptions.AddMenuItem("Change sound setting", menuMain);
            menuOptions.AddMenuItem("Back to Main menu", menuMain);

            activeMenu = menuMain;
            menuMain.WakeUp();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState keybState = Keyboard.GetState();

            if (menusRunning)
            {
                foreach (MenuWindow currentMenu in menuList)
                    currentMenu.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
                MenuInput(keybState, gamePadState);
            }
            else
            {
            }

            lastKeybState = keybState;
            lastGamePadState = gamePadState;

            base.Update(gameTime);
        }

        private void MenuInput(KeyboardState currentKeybState, GamePadState currentGamePadState)
        {
            MenuWindow newActive = activeMenu.ProcessInput(lastKeybState, currentKeybState, lastGamePadState, currentGamePadState);

            if (newActive == startGameEasy)
            {
                //set level to easy
                menusRunning = false;
            }
            else if (newActive == startGameNormal)
            {
                //set level to normal
                menusRunning = false;
            }
            else if (newActive == startGameHard)
            {
                //set level to hard
                menusRunning = false;
            }
            else if (newActive == null)
                this.Exit();
            else if (newActive != activeMenu)
                newActive.WakeUp();

            activeMenu = newActive;
        }
        
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            if (menusRunning)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                foreach (MenuWindow currentMenu in menuList)
                    currentMenu.Draw(spriteBatch);
                spriteBatch.End();
                Window.Title = "Menu running ...";
            }
            else
            {
                device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Red, 1, 0);
                Window.Title = "Game running ...";
            }

            List<string> ppEffectsList = new List<string>();
            ppEffectsList.Add("HorBlur");
            ppEffectsList.Add("VerBlur");
            float blurRadius = (float)Math.Cos(gameTime.TotalGameTime.TotalMilliseconds / 1000.0f);
            if (blurRadius < 0)
                blurRadius = 0;
            postProcessor.Parameters["xBlurSize"].SetValue(blurRadius/5.0f);
            postProcessor.PostProcess(ppEffectsList);

            base.Draw(gameTime);
        }
    }
}
