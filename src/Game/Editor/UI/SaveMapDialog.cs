using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Simple modal dialog to input a filename for saving maps.
    /// Very small feature set: capture basic letters, numbers, dash, underscore and dot.
    /// Submit with Enter, cancel with Escape.
    /// </summary>
    public class SaveMapDialog
    {
        private Rectangle _bounds;
        private string _text = "";
        private bool _isOpen = false;

        public bool IsOpen => _isOpen;
        public string Text => _text;

        public event Action<string> OnSubmit;
        public event Action OnCancel;

        public void Open(string initialText, Rectangle screenBounds)
        {
            _text = initialText ?? "";
            _isOpen = true;
            int w = Math.Min(600, screenBounds.Width - 40);
            int h = 120;
            _bounds = new Rectangle((screenBounds.Width - w) / 2, (screenBounds.Height - h) / 2, w, h);
        }

        public void Close()
        {
            _isOpen = false;
        }

        public void Update(GameTime gameTime, KeyboardState keys, KeyboardState prevKeys)
        {
            if (!_isOpen) return;

            // Submit
            if (keys.IsKeyDown(Keys.Enter) && !prevKeys.IsKeyDown(Keys.Enter))
            {
                OnSubmit?.Invoke(_text);
                Close();
                return;
            }

            // Cancel
            if (keys.IsKeyDown(Keys.Escape) && !prevKeys.IsKeyDown(Keys.Escape))
            {
                OnCancel?.Invoke();
                Close();
                return;
            }

            // Backspace
            if (keys.IsKeyDown(Keys.Back) && !prevKeys.IsKeyDown(Keys.Back))
            {
                if (_text.Length > 0) _text = _text.Substring(0, _text.Length - 1);
            }

            // Handle alphanumeric keys (single press detection)
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                if (k == Keys.Enter || k == Keys.Escape || k == Keys.Back) continue;

                if (keys.IsKeyDown(k) && !prevKeys.IsKeyDown(k))
                {
                    char? c = KeyToChar(k, keys);
                    if (c.HasValue)
                    {
                        _text += c.Value;
                    }
                }
            }
        }

        private char? KeyToChar(Keys key, KeyboardState keys)
        {
            // Letters
            if (key >= Keys.A && key <= Keys.Z)
            {
                bool shift = keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift);
                char c = (char)('a' + (key - Keys.A));
                if (shift) c = Char.ToUpper(c);
                return c;
            }

            // Numbers (top row)
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                char c = (char)('0' + (key - Keys.D0));
                return c;
            }

            // Numpad numbers
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                char c = (char)('0' + (key - Keys.NumPad0));
                return c;
            }

            // Common punctuation
            if (key == Keys.OemPeriod) return '.';
            if (key == Keys.OemMinus) return '-';
            if (key == Keys.OemPlus) return '_';
            if (key == Keys.OemComma) return ',';
            if (key == Keys.Space) return ' ';

            return null;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel)
        {
            if (!_isOpen) return;

            // Background
            spriteBatch.Draw(pixel, _bounds, new Color(10, 10, 10, 220));
            // Border
            Rectangle border = new Rectangle(_bounds.X - 2, _bounds.Y - 2, _bounds.Width + 4, _bounds.Height + 4);
            spriteBatch.Draw(pixel, border, Color.Black);

            if (font != null)
            {
                spriteBatch.DrawString(font, "Save Map As:", new Vector2(_bounds.X + 12, _bounds.Y + 12), Color.White);
                spriteBatch.DrawString(font, _text + "_", new Vector2(_bounds.X + 12, _bounds.Y + 44), Color.LightGray);

                string hint = "Enter = Save    Esc = Cancel    Allowed: letters, numbers, -, _, .";
                spriteBatch.DrawString(font, hint, new Vector2(_bounds.X + 12, _bounds.Bottom - 28), Color.Gray);
            }
        }
    }
}
