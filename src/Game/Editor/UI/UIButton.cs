using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Basic button with text/icon and hover/pressed states.
    /// NO HOTKEY TEXT - clean UI only!
    /// </summary>
    public class UIButton : UIElement
    {
        public string Text { get; set; }
        public Texture2D Icon { get; set; }
        public Color NormalColor { get; set; } = new Color(70, 70, 70);
        public Color HoverColor { get; set; } = new Color(100, 100, 100);
        public Color PressedColor { get; set; } = new Color(50, 50, 50);
        public Color TextColor { get; set; } = Color.White;
        public Action OnClick { get; set; }

        private bool _wasPressed;

        public override void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            if (!Visible || !Enabled) return;

            bool isMouseOver = IsMouseOver(mouseState);
            bool isPressed = mouseState.LeftButton == ButtonState.Pressed && isMouseOver;

            // Detect click (release after press)
            if (isMouseOver && !isPressed && _wasPressed)
            {
                OnClick?.Invoke();
            }

            _wasPressed = isPressed;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!Visible) return;

            Rectangle globalBounds = GlobalBounds;

            // Determine color based on state
            MouseState mouseState = Mouse.GetState();
            bool isMouseOver = IsMouseOver(mouseState);
            bool isPressed = mouseState.LeftButton == ButtonState.Pressed && isMouseOver;

            Color currentColor = NormalColor;
            if (isMouseOver)
            {
                currentColor = isPressed ? PressedColor : HoverColor;
            }

            // Draw background
            spriteBatch.Draw(pixelTexture, globalBounds, currentColor);

            // Draw icon if provided
            if (Icon != null)
            {
                Rectangle iconRect = new Rectangle(
                    globalBounds.X + 4,
                    globalBounds.Y + (globalBounds.Height - 24) / 2,
                    24, 24
                );
                spriteBatch.Draw(Icon, iconRect, Color.White);
            }

            // Draw text
            if (font != null && !string.IsNullOrEmpty(Text))
            {
                // Sanitize text to prevent SpriteFont crashes from unsupported Unicode characters
                string safeText = FontUtil.SanitizeForFont(font, Text);
                Vector2 textSize = font.MeasureString(safeText);
                int textX = Icon != null ? globalBounds.X + 32 : globalBounds.X + (int)((globalBounds.Width - textSize.X) / 2);
                Vector2 textPos = new Vector2(
                    textX,
                    globalBounds.Y + (globalBounds.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, safeText, textPos, TextColor);
            }

            // Border
            DrawBorder(spriteBatch, pixelTexture, globalBounds, new Color(40, 40, 40), 1);
        }

        protected void DrawBorder(SpriteBatch sb, Texture2D px, Rectangle rect, Color color, int width)
        {
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, width), color); // Top
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - width, rect.Width, width), color); // Bottom
            sb.Draw(px, new Rectangle(rect.X, rect.Y, width, rect.Height), color); // Left
            sb.Draw(px, new Rectangle(rect.Right - width, rect.Y, width, rect.Height), color); // Right
        }
    }
}
