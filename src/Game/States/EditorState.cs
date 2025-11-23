using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Editor;
using CubeSurvivor.Game.Editor.Tools;
using CubeSurvivor.Game.Editor.UI;
using CubeSurvivor.Game.Editor.Diagnostics;
using CubeSurvivor.Game.Map;

namespace CubeSurvivor.Game.States
{
    /// <summary>
    /// NEW Professional Map Editor State - Tiled/LDtk style!
    /// Mouse-first, sidebar-driven, zero hotkey spam.
    /// </summary>
    public sealed class EditorState : IGameState
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixelTexture;

        // Core editor systems
        private EditorContext _context;
        private EditorCameraController _camera;
        private EditorRenderer _renderer;
        private Dictionary<ToolType, IEditorTool> _tools;
        private Core.TextureManager _textureManager;

        // UI
        private LeftSidebar _leftSidebar;
        private RightSidebar _rightSidebar;
        private TopBar _topBar;
        private UIPauseMenu _pauseMenu;

        // Layout
        private Rectangle _canvasBounds;
        private Rectangle _leftSidebarBounds;
        private Rectangle _rightSidebarBounds;
        private Rectangle _topBarBounds;

        // Input
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;
        private bool _isMouseDown;
        private Point? _currentHoverTile;

        // Map file path
        private string _mapFilePath;

        public event Action OnReturnToMenu;

        public EditorState(
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            SpriteFont font,
            Texture2D pixelTexture,
            ChunkedTileMap map,
            MapDefinition mapDefinition,
            string mapFilePath)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _pixelTexture = pixelTexture ?? throw new ArgumentNullException(nameof(pixelTexture));

            _context = new EditorContext
            {
                Map = map,
                MapDefinition = mapDefinition
            };

            _mapFilePath = mapFilePath;

            // Initialize texture manager
            _textureManager = new Core.TextureManager(_graphicsDevice);
            _context.TextureManager = _textureManager;

            // Initialize layer visibility arrays
            _context.TileLayerVisible = new bool[mapDefinition.TileLayers.Count];
            _context.BlockLayerVisible = new bool[mapDefinition.BlockLayers.Count];
            _context.ItemLayerVisible = new bool[2] { true, true }; // ItemsLow, ItemsHigh
            
            // Set all layers visible by default
            for (int i = 0; i < _context.TileLayerVisible.Length; i++)
                _context.TileLayerVisible[i] = true;
            for (int i = 0; i < _context.BlockLayerVisible.Length; i++)
                _context.BlockLayerVisible[i] = true;

            _camera = new EditorCameraController();
            _context.Camera = _camera; // Wire camera to context!
            _renderer = new EditorRenderer();

            // Create all tools
            _tools = new Dictionary<ToolType, IEditorTool>
            {
                { ToolType.Brush, new BrushTool() },
                { ToolType.Eraser, new EraserTool() },
                { ToolType.BoxFill, new BoxFillTool() },
                { ToolType.FloodFill, new FloodFillTool() },
                { ToolType.Picker, new PickerTool() },
                { ToolType.Region, new RegionTool() },
                { ToolType.SelectMove, new SelectMoveTool() }
            };

            CalculateLayout();
            BuildUI();
            BuildPauseMenu();
            
            EditorLogger.Log("EditorState", $"Initialized. Map: {System.IO.Path.GetFileName(mapFilePath)}, Size: {mapDefinition.MapWidth}x{mapDefinition.MapHeight}");
        }

        private void BuildPauseMenu()
        {
            _pauseMenu = new UIPauseMenu(showSaveButton: true);
            
            _pauseMenu.OnResume = () =>
            {
                EditorLogger.Log("EditorState", "Resumed from pause menu");
            };
            
            _pauseMenu.OnSave = () =>
            {
                EditorLogger.Log("EditorState", "Save requested from pause menu");
                HandleSave();
            };
            
            _pauseMenu.OnMainMenu = () =>
            {
                EditorLogger.Log("EditorState", "Return to main menu from pause menu");
                if (_context.IsDirty)
                {
                    EditorLogger.Log("EditorState", "Auto-saving before exit...");
                    HandleSave();
                }
                OnReturnToMenu?.Invoke();
            };
            
            _pauseMenu.OnExitGame = () =>
            {
                EditorLogger.Log("EditorState", "Exit game requested from pause menu");
                if (_context.IsDirty)
                {
                    EditorLogger.Log("EditorState", "Auto-saving before exit...");
                    HandleSave();
                }
                System.Environment.Exit(0);
            };
        }

        private void CalculateLayout()
        {
            int screenWidth = _graphicsDevice.Viewport.Width;
            int screenHeight = _graphicsDevice.Viewport.Height;

            int leftSidebarWidth = 260;
            int rightSidebarWidth = 300;
            int topBarHeight = 50;

            _topBarBounds = new Rectangle(0, 0, screenWidth, topBarHeight);
            _leftSidebarBounds = new Rectangle(0, topBarHeight, leftSidebarWidth, screenHeight - topBarHeight);
            _rightSidebarBounds = new Rectangle(screenWidth - rightSidebarWidth, topBarHeight, rightSidebarWidth, screenHeight - topBarHeight);
            _canvasBounds = new Rectangle(leftSidebarWidth, topBarHeight, screenWidth - leftSidebarWidth - rightSidebarWidth, screenHeight - topBarHeight);
        }

        private void BuildUI()
        {
            _leftSidebar = new LeftSidebar();
            _leftSidebar.Build(_leftSidebarBounds, _context);

            _rightSidebar = new RightSidebar();
            _rightSidebar.Build(_rightSidebarBounds, _context);

            _topBar = new TopBar();
            _topBar.Build(_topBarBounds, System.IO.Path.GetFileNameWithoutExtension(_mapFilePath));
            _topBar.OnSave += HandleSave;
            _topBar.OnExit += HandleExit;
            _topBar.OnFullscreen += HandleFullscreen;
        }

        public void Enter()
        {
            EditorLogger.Log("EditorState", $"=== ENTERING EDITOR ===");
            EditorLogger.Log("EditorState", $"Map: {_mapFilePath}");
            EditorLogger.Log("EditorState", $"ActiveTool: {_context.ActiveTool}");
            EditorLogger.Log("EditorState", $"EditMode: {_context.EditMode}");
            EditorLogger.Log("EditorState", $"ActiveBrushId: {_context.ActiveBrushId}");
            _previousMouseState = Mouse.GetState();
            _previousKeyboardState = Keyboard.GetState();
        }

        public void Exit()
        {
            EditorLogger.Log("EditorState", "=== EXITING EDITOR ===");
        }

        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            // ESC to toggle pause menu (detect press, not hold)
            bool escPressed = keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape);
            if (escPressed)
            {
                _pauseMenu.Toggle(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            }

            // Update pause menu first (blocks input when open)
            _pauseMenu.Update(gameTime, mouseState, _previousMouseState);
            
            // If pause menu is open, don't update game/editor
            if (_pauseMenu.IsOpen)
            {
                _previousMouseState = mouseState;
                _previousKeyboardState = keyboardState;
                return;
            }

            // Update camera (pan/zoom)
            _camera.Update(mouseState, _previousMouseState, _canvasBounds);

            // Update UI
            _leftSidebar.Update(gameTime, mouseState, _previousMouseState, _context);
            _rightSidebar.Update(gameTime, mouseState, _previousMouseState, _context);
            _topBar.Update(gameTime, mouseState, _previousMouseState);

            // Canvas input (only if not over UI)
            bool isOverUI = _leftSidebar.HitTest(mouseState.Position) ||
                            _rightSidebar.HitTest(mouseState.Position) ||
                            _topBar.HitTest(mouseState.Position);

            if (isOverUI)
            {
                // Mouse over UI - don't process canvas input
                _currentHoverTile = null;
                if (_isMouseDown)
                {
                    _isMouseDown = false;
                }
            }
            else if (_canvasBounds.Contains(mouseState.Position))
            {
                // Mouse over canvas - process tool input
                
                // Calculate hover tile using context helpers
                Point tilePos = _context.ScreenToTile(mouseState.Position, _canvasBounds);
                _currentHoverTile = tilePos;

                // Log mouse events for debugging
                bool leftPressed = mouseState.LeftButton == ButtonState.Pressed;
                bool leftWasPressed = _previousMouseState.LeftButton == ButtonState.Pressed;
                bool leftJustPressed = leftPressed && !leftWasPressed;
                bool leftJustReleased = !leftPressed && leftWasPressed;

                if (leftJustPressed)
                {
                    EditorLogger.Log("Input", $"Canvas LMB DOWN at screen={mouseState.Position} tile={tilePos} tool={_context.ActiveTool}");
                }

                // Tool input
                IEditorTool activeTool = _tools[_context.ActiveTool];

                if (leftPressed)
                {
                    if (!_isMouseDown)
                    {
                        activeTool.OnMouseDown(tilePos, mouseState, _context);
                        _isMouseDown = true;
                    }
                    else
                    {
                        activeTool.OnMouseDrag(tilePos, mouseState, _context);
                    }
                }
                else
                {
                    if (_isMouseDown)
                    {
                        EditorLogger.Log("Input", $"Canvas LMB UP at tile={tilePos}");
                        activeTool.OnMouseUp(tilePos, mouseState, _context);
                        _isMouseDown = false;
                    }
                }
            }
            else
            {
                // Mouse outside both UI and canvas
                _currentHoverTile = null;
                if (_isMouseDown)
                {
                    _isMouseDown = false;
                }
            }

            _previousMouseState = mouseState;
            _previousKeyboardState = keyboardState;
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.Clear(new Color(20, 20, 20));

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Draw canvas (map + grid + overlays)
            DrawCanvas();

            // Draw UI
            _leftSidebar.Draw(_spriteBatch, _font, _pixelTexture);
            _rightSidebar.Draw(_spriteBatch, _font, _pixelTexture);
            _topBar.Draw(_spriteBatch, _font, _pixelTexture, System.IO.Path.GetFileNameWithoutExtension(_mapFilePath));

            // Draw debug overlay (on-screen info)
            DrawDebugOverlay();

            // Dirty indicator
            if (_context.IsDirty && !_pauseMenu.IsOpen)
            {
                _spriteBatch.DrawString(_font, "*UNSAVED*", new Vector2(10, _graphicsDevice.Viewport.Height - 30), Color.Red);
            }

            // Draw pause menu on top of everything
            _pauseMenu.Draw(_spriteBatch, _font, _pixelTexture);

            _spriteBatch.End();
        }

        private void DrawDebugOverlay()
        {
            if (_font == null) return;

            int x = _canvasBounds.X + 10;
            int y = _canvasBounds.Y + 10;
            int lineHeight = 20;

            // Background for readability
            Rectangle bgRect = new Rectangle(x - 5, y - 5, 300, lineHeight * 6 + 10);
            _spriteBatch.Draw(_pixelTexture, bgRect, new Color(0, 0, 0, 180));

            // Debug info
            _spriteBatch.DrawString(_font, $"Tool: {_context.ActiveTool}", new Vector2(x, y), Color.Cyan);
            y += lineHeight;

            _spriteBatch.DrawString(_font, $"Mode: {_context.EditMode}", new Vector2(x, y), Color.Cyan);
            y += lineHeight;

            _spriteBatch.DrawString(_font, $"BrushId: {_context.ActiveBrushId}", new Vector2(x, y), Color.Cyan);
            y += lineHeight;

            _spriteBatch.DrawString(_font, $"Layer: {_context.ActiveLayerIndex}", new Vector2(x, y), Color.Cyan);
            y += lineHeight;

            if (_currentHoverTile.HasValue)
            {
                _spriteBatch.DrawString(_font, $"Mouse Tile: {_currentHoverTile.Value}", new Vector2(x, y), Color.Yellow);
                y += lineHeight;

                // Show what's currently at this tile
                if (_context.Map != null)
                {
                    if (_context.EditMode == EditMode.Tiles)
                    {
                        int tileId = _context.Map.GetTileAt(_currentHoverTile.Value.X, _currentHoverTile.Value.Y, _context.ActiveLayerIndex);
                        _spriteBatch.DrawString(_font, $"Current: TileId={tileId}", new Vector2(x, y), Color.White);
                    }
                    else
                    {
                        BlockType blockType = _context.Map.GetBlockAtTile(_currentHoverTile.Value.X, _currentHoverTile.Value.Y, _context.ActiveLayerIndex);
                        _spriteBatch.DrawString(_font, $"Current: {blockType}", new Vector2(x, y), Color.White);
                    }
                }
            }
            else
            {
                _spriteBatch.DrawString(_font, "Mouse: (outside canvas)", new Vector2(x, y), Color.Gray);
            }
        }

        private void DrawCanvas()
        {
            // ===================================================================
            // DRAW ORDER (painter's algorithm - back to front):
            // 1. Tiles (ground layer)
            // 2. Blocks (obstacles)
            // 3. Grid (overlay)
            // 4. Regions (overlay)
            // 5. Hover highlight (overlay)
            // 6. Ghost preview (overlay)
            // 7. Tool overlay
            // ===================================================================

            // CORRECT RENDER ORDER:
            // 1. Tiles (ground)
            _renderer.DrawTiles(_spriteBatch, _pixelTexture, _context, _camera, _canvasBounds);

            // 2. ItemsLow (items below blocks)
            if (_context.MapDefinition?.ItemLayers != null && _context.MapDefinition.ItemLayers.Count > 0)
            {
                _renderer.DrawItems(_spriteBatch, _pixelTexture, _context, _camera, _canvasBounds, 0);
            }

            // 3. Blocks (collision obstacles)
            _renderer.DrawBlocks(_spriteBatch, _pixelTexture, _context, _camera, _canvasBounds);

            // 4. ItemsHigh (items above blocks)
            if (_context.MapDefinition?.ItemLayers != null && _context.MapDefinition.ItemLayers.Count > 1)
            {
                _renderer.DrawItems(_spriteBatch, _pixelTexture, _context, _camera, _canvasBounds, 1);
            }

            // Draw grid overlay
            _renderer.DrawGrid(_spriteBatch, _pixelTexture, _context, _camera, _canvasBounds);

            // Draw regions overlay
            _renderer.DrawRegions(_spriteBatch, _pixelTexture, _context, _camera, _canvasBounds, _font);

            // Draw hover highlight
            if (_currentHoverTile.HasValue)
            {
                _renderer.DrawHoverHighlight(_spriteBatch, _pixelTexture, _currentHoverTile.Value, _context, _camera, _canvasBounds);
                _renderer.DrawGhostPreview(_spriteBatch, _pixelTexture, _currentHoverTile.Value, _context, _camera, _canvasBounds, null);
            }

            // Draw tool overlay
            IEditorTool activeTool = _tools[_context.ActiveTool];
            activeTool.Draw(_spriteBatch, _pixelTexture, _font, _context, _camera, _canvasBounds);
        }

        private void HandleSave()
        {
            EditorLogger.Log("EditorState", $"=== SAVE REQUESTED ===");
            EditorLogger.Log("EditorState", $"Path: {_mapFilePath}");
            EditorLogger.Log("EditorState", $"Dirty: {_context.IsDirty}");
            
            try
            {
                // MapDefinition is already updated by tools writing to ChunkedTileMap.Definition
                MapSaver.Save(_mapFilePath, _context.MapDefinition);
                _context.IsDirty = false;
                EditorLogger.Log("EditorState", "Map saved successfully!");
            }
            catch (Exception ex)
            {
                EditorLogger.LogError("EditorState", $"Save failed: {ex.Message}");
            }
        }


        private void HandleExit()
        {
            EditorLogger.Log("EditorState", "Exit button clicked - showing pause menu");
            _pauseMenu.Open(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        }

        private void HandleFullscreen()
        {
            EditorLogger.Log("EditorState", "Fullscreen toggle requested (not implemented yet - needs Game1 wire-up)");
            // TODO: Wire this through Game1 to toggle _graphics.IsFullScreen
            // For now, just recalculate layout in case window was resized
            RecalculateLayout();
        }

        private void RecalculateLayout()
        {
            EditorLogger.Log("EditorState", "Recalculating layout...");
            CalculateLayout();
            
            // Rebuild UI with new bounds
            _leftSidebar.Build(_leftSidebarBounds, _context);
            _rightSidebar.Build(_rightSidebarBounds, _context);
            _topBar.Build(_topBarBounds, System.IO.Path.GetFileNameWithoutExtension(_mapFilePath));
            
            // Re-wire events
            _topBar.OnSave += HandleSave;
            _topBar.OnExit += HandleExit;
            _topBar.OnFullscreen += HandleFullscreen;
        }
    }
}

