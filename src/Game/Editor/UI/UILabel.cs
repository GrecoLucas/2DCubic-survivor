using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Simple text label UI element.
    /// </summary>
    public class UILabel : UIElement
    {
        public string Text { get; set; }
        public Color TextColor { get; set; } = Color.White;
        public bool CenterHorizontal { get; set; }
        public bool CenterVertical { get; set; }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!Visible || font == null || string.IsNullOrEmpty(Text)) return;

            Rectangle globalBounds = GlobalBounds;
            Vector2 textSize = font.MeasureString(Text);

            float x = globalBounds.X;
            float y = globalBounds.Y;

            if (CenterHorizontal)
            {
                x = globalBounds.X + (globalBounds.Width - textSize.X) / 2;
            }

            if (CenterVertical)
            {
                y = globalBounds.Y + (globalBounds.Height - textSize.Y) / 2;
            }

            spriteBatch.DrawString(font, Text, new Vector2(x, y), TextColor);
        }
    }
}

