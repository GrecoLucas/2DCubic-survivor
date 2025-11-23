using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Utility for sanitizing text to ensure it only contains characters supported by a SpriteFont.
    /// Prevents ArgumentException crashes when drawing text with unsupported Unicode characters.
    /// </summary>
    public static class FontUtil
    {
        /// <summary>
        /// Sanitizes text by replacing any characters not present in the font's glyph set.
        /// </summary>
        /// <param name="font">The SpriteFont to check against.</param>
        /// <param name="text">The text to sanitize.</param>
        /// <param name="replacement">Character to use for unsupported characters. Defaults to '?'.</param>
        /// <returns>Sanitized text containing only supported characters.</returns>
        public static string SanitizeForFont(SpriteFont font, string text, char replacement = '?')
        {
            if (string.IsNullOrEmpty(text) || font == null) 
                return text ?? string.Empty;

            // If font has no character set defined, return as-is (legacy fonts)
            if (font.Characters == null || font.Characters.Count == 0)
                return text;

            var sb = new StringBuilder(text.Length);
            
            foreach (var ch in text)
            {
                if (font.Characters.Contains(ch))
                {
                    sb.Append(ch);
                }
                else if (font.DefaultCharacter.HasValue)
                {
                    sb.Append(font.DefaultCharacter.Value);
                }
                else
                {
                    sb.Append(replacement);
                }
            }
            
            return sb.ToString();
        }
    }
}

