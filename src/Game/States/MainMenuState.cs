using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Editor.UI;
using CubeSurvivor.Game.Map;

namespace CubeSurvivor.Game.States
{
    /// <summary>
    /// NEW Professional Main Menu with Map Browser.
    /// Mouse-first, with previews, scroll, and clean UX.
    /// </summary>
    public sealed class MainMenuState : IGameState
    {
        private enum MenuMode
        {
            MainMenu,
            MapBrowser
        }

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixelTexture;

        private MenuMode _currentMode = MenuMode.MainMenu;
        private string _browserMode; // "play" or "edit"

        // Main Menu UI
        private UIPanel _mainMenuPanel;
        private UIButton _playButton;
        private UIButton _editorButton;
        private UIButton _newMapButton;
        private UIButton _exitButton;

        // Map Browser UI
        private UIPanel _browserPanel;
        private UIScrollList _mapsList;
        private UIButton _backButton;
        private UIButton _refreshButton;
        private MapPreviewRenderer _previewRenderer;
        private List<MapRegistry.MapInfo> _availableMaps;

        // Modal for delete confirmation
        private UIModal _deleteModal;
        private string _mapToDelete;

        // Events
        public event Action<string> OnPlayMap;
        public event Action<string> OnEditMap;
        public event Action<string> OnNewMapCreated;
        public event Action OnExit;

        private MouseState _previousMouseState;

        public MainMenuState(
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            SpriteFont font,
            Texture2D pixelTexture)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _font = font;
            _pixelTexture = pixelTexture;

            _previewRenderer = new MapPreviewRenderer(graphicsDevice);

            BuildMainMenu();
            BuildMapBrowser();
            BuildDeleteModal();
        }

        private void BuildMainMenu()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            _mainMenuPanel = new UIPanel
            {
                Bounds = new Rectangle(0, 0, screenWidth, screenHeight),
                BackgroundColor = new Color(20, 20, 30)
            };

            int buttonWidth = 400;
            int buttonHeight = 70;
            int spacing = 20;
            int startY = screenHeight / 2 - (4 * (buttonHeight + spacing)) / 2;

            _playButton = new UIButton
            {
                Bounds = new Rectangle(screenWidth / 2 - buttonWidth / 2, startY, buttonWidth, buttonHeight),
                Text = "PLAY",
                NormalColor = new Color(60, 120, 180),
                HoverColor = new Color(80, 140, 200),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine("[MainMenu] PLAY clicked");
                    _browserMode = "play";
                    RefreshMapList();
                    _currentMode = MenuMode.MapBrowser;
                }
            };

            _editorButton = new UIButton
            {
                Bounds = new Rectangle(screenWidth / 2 - buttonWidth / 2, startY + (buttonHeight + spacing), buttonWidth, buttonHeight),
                Text = "MAP EDITOR",
                NormalColor = new Color(120, 60, 180),
                HoverColor = new Color(140, 80, 200),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine("[MainMenu] MAP EDITOR clicked");
                    _browserMode = "edit";
                    RefreshMapList();
                    _currentMode = MenuMode.MapBrowser;
                }
            };

            _newMapButton = new UIButton
            {
                Bounds = new Rectangle(screenWidth / 2 - buttonWidth / 2, startY + 2 * (buttonHeight + spacing), buttonWidth, buttonHeight),
                Text = "NEW MAP",
                NormalColor = new Color(60, 180, 120),
                HoverColor = new Color(80, 200, 140),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine("[MainMenu] NEW MAP clicked");
                    CreateNewMap();
                }
            };

            _exitButton = new UIButton
            {
                Bounds = new Rectangle(screenWidth / 2 - buttonWidth / 2, startY + 3 * (buttonHeight + spacing), buttonWidth, buttonHeight),
                Text = "EXIT",
                NormalColor = new Color(180, 60, 60),
                HoverColor = new Color(200, 80, 80),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine("[MainMenu] EXIT clicked");
                    OnExit?.Invoke();
                }
            };

            _mainMenuPanel.AddChild(_playButton);
            _mainMenuPanel.AddChild(_editorButton);
            _mainMenuPanel.AddChild(_newMapButton);
            _mainMenuPanel.AddChild(_exitButton);
        }

        private void BuildMapBrowser()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            _browserPanel = new UIPanel
            {
                Bounds = new Rectangle(0, 0, screenWidth, screenHeight),
                BackgroundColor = new Color(25, 25, 35)
            };

            // Title bar with buttons
            int topBarHeight = 60;
            
            _backButton = new UIButton
            {
                Bounds = new Rectangle(20, 10, 120, 40),
                Text = "< BACK",
                NormalColor = new Color(80, 80, 80),
                HoverColor = new Color(100, 100, 100),
                OnClick = () =>
                {
                    Console.WriteLine("[MainMenu] Back to main menu");
                    _currentMode = MenuMode.MainMenu;
                }
            };

            _refreshButton = new UIButton
            {
                Bounds = new Rectangle(160, 10, 140, 40),
                Text = "REFRESH",
                NormalColor = new Color(60, 100, 140),
                HoverColor = new Color(80, 120, 160),
                OnClick = () =>
                {
                    Console.WriteLine("[MainMenu] Refresh maps");
                    RefreshMapList();
                }
            };

            // Scrollable map list
            _mapsList = new UIScrollList
            {
                Bounds = new Rectangle(40, topBarHeight + 20, screenWidth - 80, screenHeight - topBarHeight - 40),
                ItemHeight = 260,
                BackgroundColor = new Color(30, 30, 40, 220)
            };

            _browserPanel.AddChild(_backButton);
            _browserPanel.AddChild(_refreshButton);
            _browserPanel.AddChild(_mapsList);
        }

        private void BuildDeleteModal()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            _deleteModal = new UIModal(
                new Rectangle(screenWidth / 2 - 300, screenHeight / 2 - 150, 600, 300),
                "Delete Map",
                "Are you sure you want to delete this map?"
            );

            _deleteModal.OnConfirm = () =>
            {
                if (!string.IsNullOrEmpty(_mapToDelete))
                {
                    Console.WriteLine($"[MainMenu] Deleting map: {_mapToDelete}");
                    MapRegistry.DeleteMap(_mapToDelete);
                    _previewRenderer.InvalidatePreview(_mapToDelete);
                    RefreshMapList();
                }
            };

            _deleteModal.OnCancel = () =>
            {
                Console.WriteLine("[MainMenu] Delete cancelled");
            };
        }

        private void RefreshMapList()
        {
            Console.WriteLine("[MainMenu] Refreshing map list...");
            
            _availableMaps = MapRegistry.ScanMaps();
            _mapsList.Clear();

            foreach (var mapInfo in _availableMaps)
            {
                var mapCard = CreateMapCard(mapInfo);
                _mapsList.AddItem(mapCard); // Use AddItem to set Parent
            }

            Console.WriteLine($"[MainMenu] Loaded {_availableMaps.Count} maps into browser");
        }

        private UIElement CreateMapCard(MapRegistry.MapInfo mapInfo)
        {
            // Card panel - will be positioned by UIScrollList in LOCAL coords
            var card = new UIPanel
            {
                BackgroundColor = mapInfo.IsValid ? new Color(45, 45, 55, 255) : new Color(80, 40, 40, 255),
                Bounds = new Rectangle(0, 0, 800, 240) // Temp size, UIScrollList will set width
            };

            // Preview image (LOCAL coords: top-left of card)
            var preview = new UIImage
            {
                Bounds = new Rectangle(10, 10, 220, 220),
                Texture = _previewRenderer.GetPreview(mapInfo.Path),
                FallbackColor = Color.DarkRed
            };
            card.AddChild(preview);

            // Map name label
            var nameLabel = new UILabel
            {
                Bounds = new Rectangle(250, 60, 300, 25),
                Text = $"Name: {mapInfo.Name}",
                TextColor = Color.White
            };
            card.AddChild(nameLabel);

            // Size label
            var sizeLabel = new UILabel
            {
                Bounds = new Rectangle(250, 85, 300, 25),
                Text = $"Size: {mapInfo.WidthTiles}x{mapInfo.HeightTiles} tiles",
                TextColor = Color.LightGray
            };
            card.AddChild(sizeLabel);

            // Tile size label
            var tileLabel = new UILabel
            {
                Bounds = new Rectangle(250, 110, 300, 25),
                Text = $"TileSize: {mapInfo.TileSize}px  ChunkSize: {mapInfo.ChunkSize}",
                TextColor = Color.LightGray
            };
            card.AddChild(tileLabel);

            // Modified date label
            var dateLabel = new UILabel
            {
                Bounds = new Rectangle(250, 135, 300, 25),
                Text = $"Modified: {mapInfo.LastWriteTime:yyyy-MM-dd HH:mm}",
                TextColor = Color.DarkGray
            };
            card.AddChild(dateLabel);

            // Play/Edit button (LOCAL coords relative to card)
            var playButton = new UIButton
            {
                Bounds = new Rectangle(240, 10, 160, 45),
                Text = _browserMode == "play" ? "PLAY THIS" : "EDIT THIS",
                NormalColor = new Color(60, 120, 180),
                HoverColor = new Color(80, 140, 200),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine($"[MainMenu] ===== {_browserMode.ToUpper()} BUTTON CLICKED =====");
                    Console.WriteLine($"[MainMenu] Map: {mapInfo.Name}");
                    Console.WriteLine($"[MainMenu] Path: {mapInfo.Path}");
                    
                    if (_browserMode == "play")
                    {
                        OnPlayMap?.Invoke(mapInfo.Path);
                    }
                    else
                    {
                        OnEditMap?.Invoke(mapInfo.Path);
                    }
                }
            };
            card.AddChild(playButton);

            // Delete button (LOCAL coords)
            var deleteButton = new UIButton
            {
                Bounds = new Rectangle(410, 10, 110, 45),
                Text = "DELETE",
                NormalColor = new Color(180, 60, 60),
                HoverColor = new Color(200, 80, 80),
                TextColor = Color.White,
                OnClick = () =>
                {
                    Console.WriteLine($"[MainMenu] ===== DELETE BUTTON CLICKED =====");
                    Console.WriteLine($"[MainMenu] Map: {mapInfo.Name}");
                    _mapToDelete = mapInfo.Path;
                    _deleteModal.Open();
                }
            };
            card.AddChild(deleteButton);

            return card;
        }

        private void CreateNewMap()
        {
            // TODO: Show modal with inputs for name, width, height, tileSize, chunkSize
            // For now, create with defaults
            Console.WriteLine("[MainMenu] Creating new map with defaults...");

            string name = $"new_map_{DateTime.Now:yyyyMMdd_HHmmss}";
            var mapDef = MapLoader.CreateDefaultMap(256, 256, 128, 64);
            string path = System.IO.Path.Combine("assets", "maps", $"{name}.json");

            MapSaver.Save(path, mapDef);
            Console.WriteLine($"[MainMenu] Created new map: {path}");

            // Trigger event (can be caught by Game1 if needed)
            OnNewMapCreated?.Invoke(path);

            // Open in editor immediately
            OnEditMap?.Invoke(path);
        }

        public void Enter()
        {
            Console.WriteLine("[MainMenu] === ENTERING MAIN MENU ===");
            _currentMode = MenuMode.MainMenu;
            _previousMouseState = Mouse.GetState();
        }

        public void Exit()
        {
            Console.WriteLine("[MainMenu] === EXITING MAIN MENU ===");
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            // Update modal first (if open, blocks everything else)
            if (_deleteModal.IsOpen)
            {
                _deleteModal.Update(gameTime, mouseState, _previousMouseState);
                _previousMouseState = mouseState;
                return;
            }

            // Update current mode
            if (_currentMode == MenuMode.MainMenu)
            {
                _mainMenuPanel.Update(gameTime, mouseState, _previousMouseState);
            }
            else if (_currentMode == MenuMode.MapBrowser)
            {
                _browserPanel.Update(gameTime, mouseState, _previousMouseState);
            }

            _previousMouseState = mouseState;
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.Clear(new Color(20, 20, 30));

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Draw current mode
            if (_currentMode == MenuMode.MainMenu)
            {
                _mainMenuPanel.Draw(_spriteBatch, _font, _pixelTexture);

                // Title
                if (_font != null)
                {
                    string title = "CUBE SURVIVOR";
                    Vector2 titleSize = _font.MeasureString(title);
                    Vector2 titlePos = new Vector2(
                        _graphicsDevice.Viewport.Width / 2 - titleSize.X / 2,
                        100
                    );
                    _spriteBatch.DrawString(_font, title, titlePos, Color.White);
                }
            }
            else if (_currentMode == MenuMode.MapBrowser)
            {
                _browserPanel.Draw(_spriteBatch, _font, _pixelTexture);

                // Browser title
                if (_font != null)
                {
                    string browserTitle = _browserMode == "play" ? "SELECT MAP TO PLAY" : "SELECT MAP TO EDIT";
                    _spriteBatch.DrawString(_font, browserTitle, new Vector2(_graphicsDevice.Viewport.Width / 2 - 150, 15), Color.White);
                }

                // Map cards are now drawn through UI tree (no manual DrawMapCards needed!)
            }

            // Draw modal on top
            if (_deleteModal.IsOpen)
            {
                _deleteModal.Draw(_spriteBatch, _font, _pixelTexture);
            }

            _spriteBatch.End();
        }

        public void Dispose()
        {
            _previewRenderer?.Dispose();
        }
    }
}

