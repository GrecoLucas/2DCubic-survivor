using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Modal dialog box with title, message, and buttons.
    /// Blocks interaction with background UI.
    /// </summary>
    public class UIModal : UIElement
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public UIPanel Panel { get; private set; }
        public Action OnConfirm { get; set; }
        public Action OnCancel { get; set; }

        private UIButton _confirmButton;
        private UIButton _cancelButton;
        private bool _isOpen;

        public UIModal(Rectangle bounds, string title, string message)
        {
            Bounds = bounds;
            Title = title;
            Message = message;

            Panel = new UIPanel
            {
                Bounds = bounds,
                BackgroundColor = new Color(40, 40, 40, 250)
            };

            int buttonWidth = 120;
            int buttonHeight = 40;
            int spacing = 10;

            // Buttons use LOCAL coordinates relative to panel
            _confirmButton = new UIButton
            {
                Bounds = new Rectangle(
                    bounds.Width / 2 - buttonWidth - spacing / 2,
                    bounds.Height - buttonHeight - 20,
                    buttonWidth,
                    buttonHeight
                ),
                Text = "OK",
                NormalColor = new Color(60, 120, 60),
                HoverColor = new Color(80, 140, 80),
                OnClick = () =>
                {
                    OnConfirm?.Invoke();
                    Close();
                }
            };

            _cancelButton = new UIButton
            {
                Bounds = new Rectangle(
                    bounds.Width / 2 + spacing / 2,
                    bounds.Height - buttonHeight - 20,
                    buttonWidth,
                    buttonHeight
                ),
                Text = "Cancel",
                NormalColor = new Color(120, 60, 60),
                HoverColor = new Color(140, 80, 80),
                OnClick = () =>
                {
                    OnCancel?.Invoke();
                    Close();
                }
            };

            Panel.AddChild(_confirmButton);
            Panel.AddChild(_cancelButton);
        }

        public void Open()
        {
            _isOpen = true;
            Visible = true;
            Enabled = true;
        }

        public void Close()
        {
            _isOpen = false;
            Visible = false;
            Enabled = false;
        }

        public bool IsOpen => _isOpen;

        public override void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            if (!_isOpen) return;

            Panel.Update(gameTime, mouseState, previousMouseState);
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!_isOpen) return;

            Rectangle globalBounds = GlobalBounds;

            // Semi-transparent background overlay
            Rectangle screenRect = new Rectangle(0, 0, 10000, 10000); // Cover entire screen
            spriteBatch.Draw(pixelTexture, screenRect, new Color(0, 0, 0, 180));

            // Panel
            Panel.Draw(spriteBatch, font, pixelTexture);

            // Title
            if (font != null && !string.IsNullOrEmpty(Title))
            {
                Vector2 titlePos = new Vector2(globalBounds.X + 20, globalBounds.Y + 20);
                spriteBatch.DrawString(font, Title, titlePos, Color.White);
            }

            // Message
            if (font != null && !string.IsNullOrEmpty(Message))
            {
                Vector2 messagePos = new Vector2(globalBounds.X + 20, globalBounds.Y + 60);
                spriteBatch.DrawString(font, Message, messagePos, Color.LightGray);
            }
        }

        public override bool HitTest(Point point)
        {
            if (!_isOpen) return false;
            return true; // Modal captures all input
        }
    }
}

