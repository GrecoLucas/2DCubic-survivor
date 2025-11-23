using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Simple image/texture display UI element.
    /// </summary>
    public class UIImage : UIElement
    {
        public Texture2D Texture { get; set; }
        public Color Tint { get; set; } = Color.White;
        public Color FallbackColor { get; set; } = Color.Gray;

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!Visible) return;

            Rectangle globalBounds = GlobalBounds;

            if (Texture != null)
            {
                spriteBatch.Draw(Texture, globalBounds, Tint);
            }
            else
            {
                // Fallback colored rectangle
                spriteBatch.Draw(pixelTexture, globalBounds, FallbackColor);
            }
        }
    }
}

