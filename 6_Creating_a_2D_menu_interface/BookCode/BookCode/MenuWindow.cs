using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
    public enum WindowState { Starting, Active, Ending, Inactive }

    class MenuWindow
    {
        private struct MenuItem
        {
            public string itemText;
            public MenuWindow itemLink;

            public MenuItem(string itemText, MenuWindow itemLink)
            {
                this.itemText = itemText;
                this.itemLink = itemLink;
            }
        }

        private TimeSpan changeSpan;
        private WindowState windowState;
        private List<MenuItem> itemList;
        private int selectedItem;
        private double changeProgress;

        private SpriteFont spriteFont;
        private string menuTitle;        
        private Texture2D backgroundImage;

        public MenuWindow(SpriteFont spriteFont, string menuTitle, Texture2D backgroundImage)
        {
            itemList = new List<MenuItem>();                        
            changeSpan = TimeSpan.FromMilliseconds(800);
            selectedItem = 0;
            changeProgress = 0;
            windowState = WindowState.Inactive;

            this.spriteFont = spriteFont;
            this.menuTitle = menuTitle;
            this.backgroundImage = backgroundImage;
        }

        public void AddMenuItem(string itemText, MenuWindow itemLink)
        {
            MenuItem newItem = new MenuItem(itemText, itemLink);
            itemList.Add(newItem);
        }

        public void WakeUp()
        {
            windowState = WindowState.Starting;
        }

        public void Update(double timePassedSinceLastFrame)
        {
            if ((windowState == WindowState.Starting) || (windowState == WindowState.Ending))
                changeProgress += timePassedSinceLastFrame / changeSpan.TotalMilliseconds;
            
            if (changeProgress >= 1.0f)
            {
                changeProgress = 0.0f;
                if (windowState == WindowState.Starting)
                    windowState = WindowState.Active;
                else if (windowState == WindowState.Ending)
                    windowState = WindowState.Inactive;
            }
        }

        public MenuWindow ProcessInput(KeyboardState lastKeybState, KeyboardState currentKeybState, GamePadState lastGamePadState, GamePadState currentGamePadState)
        {
            if (lastKeybState.IsKeyUp(Keys.Down) && currentKeybState.IsKeyDown(Keys.Down))
                selectedItem++;

            if ((lastGamePadState.DPad.Down == ButtonState.Released) && (currentGamePadState.DPad.Down == ButtonState.Pressed))
                selectedItem++;

            if (lastKeybState.IsKeyUp(Keys.Up) && currentKeybState.IsKeyDown(Keys.Up))
                selectedItem--;

            if ((lastGamePadState.DPad.Up == ButtonState.Released) && (currentGamePadState.DPad.Up == ButtonState.Pressed))
                selectedItem--;

            if (selectedItem < 0)
                selectedItem = 0;

            if (selectedItem >= itemList.Count)
                selectedItem = itemList.Count-1;

            if ((lastKeybState.IsKeyUp(Keys.Enter) && currentKeybState.IsKeyDown(Keys.Enter)) || ((lastGamePadState.Buttons.A == ButtonState.Released) && (currentGamePadState.Buttons.A == ButtonState.Pressed)))
            {
                windowState = WindowState.Ending;
                return itemList[selectedItem].itemLink;
            }
            else if (lastKeybState.IsKeyDown(Keys.Escape))
                return null;
            else
                return this;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (windowState == WindowState.Inactive)
                return;

            float smoothedProgress = MathHelper.SmoothStep(0,1,(float)changeProgress);

            int verPosition = 300;
            float horPosition = 300;
            float alphaValue;
            float bgLayerDepth;
            Color bgColor;

            switch (windowState)
            {
                case WindowState.Starting:
                    horPosition -= 200 * (1.0f - (float)smoothedProgress);
                    alphaValue = smoothedProgress;
                    bgLayerDepth = 0.5f;
                    bgColor = new Color(new Vector4(1, 1, 1, alphaValue));
                    break;
                case WindowState.Ending:
                    horPosition += 200 * (float)smoothedProgress;
                    alphaValue = 1.0f - smoothedProgress;
                    bgLayerDepth = 1;
                    bgColor = Color.White;
                    break;
                default:
                    alphaValue = 1;
                    bgLayerDepth = 1;
                    bgColor = Color.White;
                    break;
            }
            
            Color titleColor = new Color(new Vector4(1, 1, 1, alphaValue));
            spriteBatch.Draw(backgroundImage, new Vector2(), null, bgColor, 0, Vector2.Zero, 1, SpriteEffects.None, bgLayerDepth);
            spriteBatch.DrawString(spriteFont, menuTitle, new Vector2(horPosition, 200), titleColor,0,Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            
            for (int itemID = 0; itemID < itemList.Count; itemID++)
            {                    
                Vector2 itemPostition = new Vector2(horPosition, verPosition);
                Color itemColor = Color.White;
                
                if (itemID == selectedItem)
                    itemColor = new Color(new Vector4(1,0,0,alphaValue));
                else
                    itemColor = new Color(new Vector4(1,1,1,alphaValue));
                
                spriteBatch.DrawString(spriteFont, itemList[itemID].itemText, itemPostition, itemColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                verPosition += 30;
            }            
        }
    }
}
