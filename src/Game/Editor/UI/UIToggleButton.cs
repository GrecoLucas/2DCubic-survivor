using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Toggle button with Selected state (for tools, palette items, etc.)
    /// </summary>
    public class UIToggleButton : UIElement
    {
        public string Text { get; set; }
        public Texture2D Icon { get; set; }
        public bool Selected { get; set; }
        public Color NormalColor { get; set; } = new Color(70, 70, 70);
        public Color HoverColor { get; set; } = new Color(100, 100, 100);
        public Color SelectedColor { get; set; } = new Color(60, 120, 180);
        public Color TextColor { get; set; } = Color.White;
        public Action OnClick { get; set; }

        private bool _wasPressed;

        public override void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            if (!Visible || !Enabled) return;

            bool isMouseOver = IsMouseOver(mouseState);
            bool isPressed = mouseState.LeftButton == ButtonState.Pressed && isMouseOver;

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
            MouseState mouseState = Mouse.GetState();
            bool isMouseOver = IsMouseOver(mouseState);

            Color currentColor = Selected ? SelectedColor : (isMouseOver ? HoverColor : NormalColor);

            // Background
            spriteBatch.Draw(pixelTexture, globalBounds, currentColor);

            // Icon
            if (Icon != null)
            {
                Rectangle iconRect = new Rectangle(
                    globalBounds.X + 4,
                    globalBounds.Y + (globalBounds.Height - 24) / 2,
                    24, 24
                );
                spriteBatch.Draw(Icon, iconRect, Color.White);
            }

            // Text
            if (font != null && !string.IsNullOrEmpty(Text))
            {
                Vector2 textSize = font.MeasureString(Text);
                int textX = Icon != null ? globalBounds.X + 32 : globalBounds.X + (int)((globalBounds.Width - textSize.X) / 2);
                Vector2 textPos = new Vector2(
                    textX,
                    globalBounds.Y + (globalBounds.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, Text, textPos, TextColor);
            }

            // Selected indicator (thick border)
            if (Selected)
            {
                DrawBorder(spriteBatch, pixelTexture, globalBounds, Color.Cyan, 2);
            }
            else
            {
                DrawBorder(spriteBatch, pixelTexture, globalBounds, new Color(40, 40, 40), 1);
            }
        }

        protected void DrawBorder(SpriteBatch sb, Texture2D px, Rectangle rect, Color color, int width)
        {
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - width, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Y, width, rect.Height), color);
            sb.Draw(px, new Rectangle(rect.Right - width, rect.Y, width, rect.Height), color);
        }
    }
}





