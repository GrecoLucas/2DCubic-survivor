using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.Editor.UI
{
    /// <summary>
    /// Pause/ESC menu with options: Resume, Save, Main Menu, Exit Game
    /// Blocks all input when open.
    /// </summary>
    public class UIPauseMenu : UIElement
    {
        private UIPanel _panel;
        private UIButton _resumeButton;
        private UIButton _saveButton;
        private UIButton _mainMenuButton;
        private UIButton _exitGameButton;
        private bool _isOpen;
        private bool _showSaveButton;

        public Action OnResume { get; set; }
        public Action OnSave { get; set; }
        public Action OnMainMenu { get; set; }
        public Action OnExitGame { get; set; }

        public bool IsOpen => _isOpen;

        public UIPauseMenu(bool showSaveButton = false)
        {
            _showSaveButton = showSaveButton;
            BuildMenu();
        }

        private void BuildMenu()
        {
            // Center panel
            int panelWidth = 400;
            int panelHeight = _showSaveButton ? 450 : 380;
            
            _panel = new UIPanel
            {
                Bounds = new Rectangle(0, 0, panelWidth, panelHeight), // Will be centered in Update
                BackgroundColor = new Color(30, 30, 30, 250)
            };

            int buttonWidth = 300;
            int buttonHeight = 50;
            int spacing = 15;
            int startY = 80;
            int centerX = (panelWidth - buttonWidth) / 2;

            // Resume button
            _resumeButton = new UIButton
            {
                Bounds = new Rectangle(centerX, startY, buttonWidth, buttonHeight),
                Text = "Resume",
                NormalColor = new Color(60, 120, 180),
                HoverColor = new Color(80, 140, 200),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine("[PauseMenu] Resume clicked");
                    Close();
                    OnResume?.Invoke();
                }
            };
            _panel.AddChild(_resumeButton);
            startY += buttonHeight + spacing;

            // Save button (only if enabled)
            if (_showSaveButton)
            {
                _saveButton = new UIButton
                {
                    Bounds = new Rectangle(centerX, startY, buttonWidth, buttonHeight),
                    Text = "Save",
                    NormalColor = new Color(60, 180, 60),
                    HoverColor = new Color(80, 200, 80),
                    TextColor = Color.White,
                    OnClick = () =>
                    {
                        Console.WriteLine("[PauseMenu] Save clicked");
                        OnSave?.Invoke();
                        // Don't close menu after save
                    }
                };
                _panel.AddChild(_saveButton);
                startY += buttonHeight + spacing;
            }

            // Main Menu button
            _mainMenuButton = new UIButton
            {
                Bounds = new Rectangle(centerX, startY, buttonWidth, buttonHeight),
                Text = "Main Menu",
                NormalColor = new Color(120, 80, 60),
                HoverColor = new Color(140, 100, 80),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine("[PauseMenu] Main Menu clicked");
                    Close();
                    OnMainMenu?.Invoke();
                }
            };
            _panel.AddChild(_mainMenuButton);
            startY += buttonHeight + spacing;

            // Exit Game button
            _exitGameButton = new UIButton
            {
                Bounds = new Rectangle(centerX, startY, buttonWidth, buttonHeight),
                Text = "Exit Game",
                NormalColor = new Color(180, 60, 60),
                HoverColor = new Color(200, 80, 80),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine("[PauseMenu] Exit Game clicked");
                    Close();
                    OnExitGame?.Invoke();
                }
            };
            _panel.AddChild(_exitGameButton);
        }

        public void Open(int screenWidth, int screenHeight)
        {
            _isOpen = true;
            Visible = true;
            Enabled = true;

            // Center panel on screen
            _panel.Bounds = new Rectangle(
                (screenWidth - _panel.Bounds.Width) / 2,
                (screenHeight - _panel.Bounds.Height) / 2,
                _panel.Bounds.Width,
                _panel.Bounds.Height
            );

            Console.WriteLine("[PauseMenu] Opened");
        }

        public void Close()
        {
            _isOpen = false;
            Visible = false;
            Enabled = false;
            Console.WriteLine("[PauseMenu] Closed");
        }

        public void Toggle(int screenWidth, int screenHeight)
        {
            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open(screenWidth, screenHeight);
            }
        }

        public override void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            if (!_isOpen) return;

            _panel.Update(gameTime, mouseState, previousMouseState);
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            if (!_isOpen) return;

            // Semi-transparent overlay
            Rectangle screenRect = new Rectangle(0, 0, 10000, 10000);
            spriteBatch.Draw(pixelTexture, screenRect, new Color(0, 0, 0, 200));

            // Panel
            _panel.Draw(spriteBatch, font, pixelTexture);

            // Title
            if (font != null)
            {
                string title = "PAUSED";
                Vector2 titleSize = font.MeasureString(title);
                Rectangle panelGlobal = _panel.GlobalBounds;
                Vector2 titlePos = new Vector2(
                    panelGlobal.X + (panelGlobal.Width - titleSize.X) / 2,
                    panelGlobal.Y + 20
                );
                spriteBatch.DrawString(font, title, titlePos, Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            }
        }

        public override bool HitTest(Point point)
        {
            // Capture all input when open
            return _isOpen;
        }
    }
}

