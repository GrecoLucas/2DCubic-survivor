using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Top bar: Save, Load, New, Exit, Fullscreen
    /// </summary>
    public class TopBar
    {
        private UIPanel _panel;
        private UIButton _saveButton;
        private UIButton _exitButton;
        private UIButton _fullscreenButton;

        public event Action OnSave;
        public event Action OnExit;
        public event Action OnFullscreen;

        public void Build(Rectangle bounds, string mapName)
        {
            _panel = new UIPanel
            {
                Bounds = bounds,
                BackgroundColor = new Color(20, 20, 20, 250)
            };

            int buttonWidth = 120;
            int buttonHeight = 32;
            int spacing = 10;
            int x = bounds.Right - spacing - buttonWidth;

            // Fullscreen button
            _fullscreenButton = new UIButton
            {
                Bounds = new Rectangle(x, bounds.Y + (bounds.Height - buttonHeight) / 2, buttonWidth, buttonHeight),
                Text = "Fullscreen",
                NormalColor = new Color(70, 70, 70),
                OnClick = () => OnFullscreen?.Invoke()
            };
            _panel.AddChild(_fullscreenButton);
            x -= buttonWidth + spacing;

            // Exit button
            _exitButton = new UIButton
            {
                Bounds = new Rectangle(x, bounds.Y + (bounds.Height - buttonHeight) / 2, buttonWidth, buttonHeight),
                Text = "SAVE & EXIT",
                NormalColor = new Color(60, 120, 60),
                OnClick = () => OnExit?.Invoke()
            };
            _panel.AddChild(_exitButton);
            x -= buttonWidth + spacing;

            // Save button
            _saveButton = new UIButton
            {
                Bounds = new Rectangle(x, bounds.Y + (bounds.Height - buttonHeight) / 2, buttonWidth, buttonHeight),
                Text = "Save",
                NormalColor = new Color(80, 120, 180),
                OnClick = () => OnSave?.Invoke()
            };
            _panel.AddChild(_saveButton);
        }

        public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            _panel.Update(gameTime, mouseState, previousMouseState);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, string mapName)
        {
            _panel.Draw(spriteBatch, font, pixelTexture);

            // Draw map name on left
            if (font != null && !string.IsNullOrEmpty(mapName))
            {
                spriteBatch.DrawString(font, $"Editing: {mapName}", new Vector2(_panel.Bounds.X + 10, _panel.Bounds.Y + 10), Color.White);
            }
        }

        public bool HitTest(Point point) => _panel.HitTest(point);
    }
}

