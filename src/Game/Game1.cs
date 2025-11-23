using System;
using System.IO;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor
{
    /// <summary>
    /// Main game class using state machine and MapDefinition.
    /// Clean SOLID architecture with no hardcoded map data.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        
        private StateManager _stateManager;

        public Game1()
        {
            Console.WriteLine("[Game1] Constructor started");
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GameConfig.ScreenWidth;
            _graphics.PreferredBackBufferHeight = GameConfig.ScreenHeight;
            
            Console.WriteLine("[Game1V2] Constructor completed");
        }

        protected override void Initialize()
        {
            Console.WriteLine("[Game1V2] Initialize() started");
            base.Initialize();
            Console.WriteLine("[Game1V2] Initialize() completed");
        }

        protected override void LoadContent()
        {
            Console.WriteLine("[Game1V2] LoadContent() started");
            
            try
            {
                // Initialize core resources
                _spriteBatch = new SpriteBatch(GraphicsDevice);
                _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
                
                // Load font
                try
                {
                    _font = Content.Load<SpriteFont>("DefaultFont");
                    Console.WriteLine("[Game1V2] Font loaded successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Game1V2] Could not load font: {ex.Message}");
                    // Create minimal fallback - game will handle null font
                }
                
                // Ensure maps directory exists
                string mapsDir = Path.Combine("assets", "maps");
                if (!Directory.Exists(mapsDir))
                {
                    Directory.CreateDirectory(mapsDir);
                    Console.WriteLine($"[Game1V2] Created directory: {mapsDir}");
                }
                
                // Initialize state manager
                _stateManager = new StateManager();
                
                // Create and enter main menu state
                var mainMenuState = CreateMainMenuState();
                _stateManager.SwitchState(mainMenuState);
                
                Console.WriteLine("[Game1V2] LoadContent() completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Game1V2] ERROR in LoadContent: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }

        private IGameState CreateMainMenuState()
        {
            var mainMenu = new MainMenuState(
                GraphicsDevice,
                _spriteBatch,
                _font,
                _pixelTexture
            );
            
            // Wire up NEW event handlers
            mainMenu.OnPlayMap += (mapPath) =>
            {
                Console.WriteLine($"[Game1] PLAY MAP: {mapPath}");
                OpenPlayState(mapPath);
            };
            
            mainMenu.OnEditMap += (mapPath) =>
            {
                Console.WriteLine($"[Game1] EDIT MAP: {mapPath}");
                OpenEditorState(mapPath);
            };
            
            mainMenu.OnNewMapCreated += (mapPath) =>
            {
                Console.WriteLine($"[Game1] NEW MAP CREATED: {mapPath}");
                OpenEditorState(mapPath);
            };
            
            mainMenu.OnExit += () =>
            {
                Console.WriteLine("[Game1] EXIT requested");
                Exit();
            };
            
            return mainMenu;
        }

        private void OpenPlayState(string mapPath)
        {
            Console.WriteLine($"[Game1] Loading PlayState for: {mapPath}");
            
            try
            {
                var playState = new PlayState(
                    GraphicsDevice,
                    _spriteBatch,
                    _font,
                    _pixelTexture,
                    mapPath
                );
                
                // Wire up exit event to return to main menu
                playState.OnReturnToMenu += () =>
                {
                    Console.WriteLine("[Game1] Returning to main menu from play state");
                    _stateManager.SwitchState(CreateMainMenuState());
                };
                
                _stateManager.SwitchState(playState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Game1] Failed to load PlayState: {ex.Message}");
                // Return to menu on error
                _stateManager.SwitchState(CreateMainMenuState());
            }
        }

        private void OpenEditorState(string mapPath)
        {
            Console.WriteLine($"[Game1] Loading EditorState for: {mapPath}");
            
            try
            {
                // Load map
                MapDefinition mapDef = MapLoader.Load(mapPath);
                if (mapDef == null)
                {
                    Console.WriteLine("[Game1] Failed to load map, creating default");
                    mapDef = MapLoader.CreateDefaultMap(256, 256, 128, 64);
                    MapSaver.Save(mapPath, mapDef);
                }
                
                var chunkedMap = new ChunkedTileMap(mapDef);
                
                var editorState = new EditorState(
                    GraphicsDevice,
                    _spriteBatch,
                    _font,
                    _pixelTexture,
                    chunkedMap,
                    mapDef,
                    mapPath
                );
                
                // Wire up exit event to return to main menu
                editorState.OnReturnToMenu += () =>
                {
                    Console.WriteLine("[Game1] Returning to main menu from editor");
                    _stateManager.SwitchState(CreateMainMenuState());
                };
                
                _stateManager.SwitchState(editorState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Game1] Failed to load EditorState: {ex.Message}");
                // Return to menu on error
                _stateManager.SwitchState(CreateMainMenuState());
            }
        }

        protected override void Update(GameTime gameTime)
        {
            _stateManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _stateManager.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}

