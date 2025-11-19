using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CubeSurvivor.Systems
{
    // Main menu simples com botão "Play"
    public sealed class MainMenu : IMenu
    {
        public event Action OnPlayRequested;

        public bool DrawAndHandle(Entity player, SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, ref MouseState previousMouseState)
        {
            if (spriteBatch == null || pixelTexture == null)
                return true;

            // Overlay e painel
            var overlayColor = new Color(0, 0, 0, 200);
            var panelColor = new Color(30, 30, 36);
            var borderColor = Color.White * 0.2f;

            int boxW = 400, boxH = 180;
            int centerX = GameConfig.ScreenWidth / 2;
            int centerY = GameConfig.ScreenHeight / 2;
            Rectangle box = new Rectangle(centerX - boxW / 2, centerY - boxH / 2, boxW, boxH);

            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), overlayColor);
            spriteBatch.Draw(pixelTexture, box, panelColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y, boxW, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y + boxH - 2, boxW, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y, 2, boxH), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X + boxW - 2, box.Y, 2, boxH), borderColor);

            // Título
            if (font != null)
            {
                string title = "CUBE SURVIVOR";
                Vector2 titleSize = font.MeasureString(title);
                Vector2 titlePos = new Vector2(centerX - titleSize.X / 2, box.Y + 16);
                spriteBatch.DrawString(font, title, titlePos, Color.Gold);
            }

            // Botão Play
            var mouse = Mouse.GetState();
            bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;

            int btnW = 260, btnH = 56;
            Rectangle btnRect = new Rectangle(centerX - btnW / 2, box.Y + 70, btnW, btnH);
            bool isHovered = btnRect.Contains(mouse.X, mouse.Y);

            DrawButton(spriteBatch, pixelTexture, font, btnRect, "Play", isHovered);

            if (isHovered && mouseClicked)
            {
                OnPlayRequested?.Invoke();
                previousMouseState = mouse;
                return false; // fechar menu
            }

            previousMouseState = mouse;
            return true; // menu continua aberto
        }

        private void DrawButton(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, Rectangle rect, string text, bool isHovered)
        {
            Color bgColor = isHovered ? Color.CornflowerBlue : Color.DimGray;
            Color textColor = isHovered ? Color.White : Color.LightGray;

            spriteBatch.Draw(pixelTexture, rect, bgColor);

            if (font != null)
            {
                Vector2 textSize = font.MeasureString(text);
                Vector2 textPos = new Vector2(
                    rect.X + (rect.Width - textSize.X) / 2,
                    rect.Y + (rect.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, text, textPos, textColor);
            }
        }
    }
}