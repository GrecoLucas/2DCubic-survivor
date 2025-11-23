using System;
using System.IO;
using System.Linq;
using CubeSurvivor.Core;
using CubeSurvivor.Core.Spatial;
using CubeSurvivor.Entities;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.UI;
using CubeSurvivor.Inventory.Systems;
using CubeSurvivor.Systems;
using CubeSurvivor.Systems.Core;
using CubeSurvivor.Systems.Rendering;
using CubeSurvivor.Systems.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Game.States
{
    /// <summary>
    /// Play state with full MapV2 integration.
    /// This is the main gameplay state.
    /// </summary>
    public sealed class PlayState : IGameState
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixelTexture;
        
        // Core ECS
        private GameWorld _world;
        
        // Map V2
        private ChunkedTileMap _chunkedMap;
        private MapRegionProvider _regionProvider;
        private BlockEntityStreamer _blockStreamer;
        private MapRenderSystem _mapRenderSystem;
        
        // Services
        private CameraService _cameraService;
        private TextureManager _textureManager;
        private SafeZoneManager _safeZoneManager;
        private ISpatialIndex _spatialIndex;
        
        // Factories
        private IPlayerFactory _playerFactory;
        private IEnemyFactory _enemyFactory;
        private IBulletFactory _bulletFactory;
        private IWorldObjectFactory _worldObjectFactory;
        
        // Systems
        private RenderSystem _renderSystem;
        private UISystem _uiSystem;
        private GameStateSystem _gameStateSystem;
        private PlayerInputSystem _inputSystem;
        private InventoryInputSystem _inventoryInputSystem;
        private InventoryUISystem _inventoryUISystem;
        private InventoryDragDropSystem _inventoryDragDropSystem;
        private ConsumptionUISystem _consumptionUISystem;
        private BulletSystem _bulletSystem;
        private BiomeSystem _biomeSystem;
        
        private string _mapPath;
        private UIPauseMenu _pauseMenu;
        private KeyboardState _previousKeyboardState;
        
        public event Action OnReturnToMenu;
        
        public PlayState(
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            SpriteFont font,
            Texture2D pixelTexture,
            string mapPath = null)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _font = font ?? throw new ArgumentNullException(nameof(font));
            _pixelTexture = pixelTexture ?? throw new ArgumentNullException(nameof(pixelTexture));
            _mapPath = mapPath;
        }

        public void Enter()
        {
            Console.WriteLine("[PlayState] Entering play state");
            
            // Initialize core systems
            InitializeCore();
            
            // Load map
            LoadMap();
            
            // Initialize gameplay systems
            InitializeSystems();
            
            // Spawn player
            SpawnPlayer();
            
            // Initialize pause menu
            InitializePauseMenu();
            
            _previousKeyboardState = Keyboard.GetState();
            
            Console.WriteLine("[PlayState] Play state initialized successfully");
        }

        private void InitializePauseMenu()
        {
            _pauseMenu = new UIPauseMenu(showSaveButton: false); // No save in play mode
            
            _pauseMenu.OnResume = () =>
            {
                Console.WriteLine("[PlayState] Resumed from pause menu");
            };
            
            _pauseMenu.OnMainMenu = () =>
            {
                Console.WriteLine("[PlayState] Return to main menu from pause menu");
                OnReturnToMenu?.Invoke();
            };
            
            _pauseMenu.OnExitGame = () =>
            {
                Console.WriteLine("[PlayState] Exit game requested from pause menu");
                System.Environment.Exit(0);
            };
        }

        public void Exit()
        {
            Console.WriteLine("[PlayState] Exiting play state");
            
            // Cleanup
            _blockStreamer?.ClearAll();
        }

        private void InitializeCore()
        {
            _world = new GameWorld();
            _textureManager = new TextureManager(_graphicsDevice);
            _spatialIndex = new SpatialHashGrid(cellSize: 128);
            _safeZoneManager = new SafeZoneManager(new WorldObjectFactory());
            
            // Initialize factories
            var weaponVisualFactory = new WeaponVisualFactory();
            _playerFactory = new PlayerFactory();
            if (_playerFactory is PlayerFactory pf)
            {
                pf.SetWeaponVisualFactory(weaponVisualFactory);
                pf.SetTextureManager(_textureManager);
            }
            
            _enemyFactory = new EnemyFactory();
            _bulletFactory = new BulletFactory();
            _worldObjectFactory = new WorldObjectFactory();
            
            // Load textures
            LoadTextures();
        }

        private void LoadTextures()
        {
            try
            {
                _textureManager.LoadTexture("grass", "grass.png");
                _textureManager.LoadTexture("cave", "cave.png");
                _textureManager.LoadTexture("player", "player.png");
                _textureManager.LoadTexture("apple", "apple.png");
                _textureManager.LoadTexture("brain", "brain.png");
                _textureManager.LoadTexture("gun", "gun.png");
                _textureManager.LoadTexture("hammer", "hammer.png");
                _textureManager.LoadTexture("wood", "wood.png");
                Console.WriteLine("[PlayState] Textures loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayStateV2] Error loading textures: {ex.Message}");
            }
        }

        private void LoadMap()
        {
            // Load or create map
            MapDefinition mapDef;
            
            if (!string.IsNullOrEmpty(_mapPath) && File.Exists(_mapPath))
            {
                Console.WriteLine($"[PlayStateV2] Loading map from: {_mapPath}");
                mapDef = MapLoader.Load(_mapPath);
            }
            else
            {
                Console.WriteLine("[PlayState] Using QuickStart to load/create map");
                mapDef = MapLoader.LoadOrCreateMap();
            }
            
            // Create runtime map
            _chunkedMap = new ChunkedTileMap(mapDef);
            _regionProvider = new MapRegionProvider(_chunkedMap);
            
            // Initialize camera with map size
            int mapWidthPixels = _chunkedMap.MapWidthInPixels;
            int mapHeightPixels = _chunkedMap.MapHeightInPixels;
            
            _cameraService = new CameraService(
                GameConfig.ScreenWidth,
                GameConfig.ScreenHeight,
                mapWidthPixels,
                mapHeightPixels
            );
            
            Console.WriteLine($"[PlayStateV2] Map loaded: {mapWidthPixels}x{mapHeightPixels}px");
            MapLoader.PrintMapSummary(mapDef);
        }

        private void InitializeSystems()
        {
            // Initialize map rendering
            _mapRenderSystem = new MapRenderSystem(
                _chunkedMap,
                _spriteBatch,
                _cameraService,
                _textureManager
            );
            
            // Initialize block streaming
            _blockStreamer = new BlockEntityStreamer(
                _chunkedMap,
                _worldObjectFactory,
                _world,
                _cameraService
            );
            
            // Initialize biome system (for backward compatibility)
            InitializeBiomeSystem();
            
            // Initialize entity rendering
            _renderSystem = new RenderSystem(_spriteBatch);
            _renderSystem.Initialize(_world);
            _renderSystem.CreatePixelTexture(_graphicsDevice);
            
            // Initialize UI systems
            _uiSystem = new UISystem(_spriteBatch, _font, _pixelTexture, null, _textureManager.GetTexture("brain"));
            _uiSystem.Initialize(_world);
            
            _gameStateSystem = new GameStateSystem();
            _gameStateSystem.Initialize(_world);
            
            // Input systems
            _inputSystem = new PlayerInputSystem(_bulletFactory);
            _inputSystem.SetScreenSize(GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            
            _inventoryInputSystem = new InventoryInputSystem();
            _inventoryInputSystem.Initialize(_world);
            
            _inventoryUISystem = new InventoryUISystem(_spriteBatch, _font, _pixelTexture);
            _inventoryUISystem.Initialize(_world);
            _inventoryUISystem.SetScreenSize(GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            
            _inventoryDragDropSystem = new InventoryDragDropSystem();
            _inventoryDragDropSystem.Initialize(_world);
            _inventoryDragDropSystem.SetScreenSize(GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            
            _consumptionUISystem = new ConsumptionUISystem(_spriteBatch, _font, _pixelTexture);
            _consumptionUISystem.Initialize(_world);
            _consumptionUISystem.SetScreenSize(GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            
            // Register gameplay systems IN CORRECT ORDER
            Console.WriteLine("[PlayState] Registering gameplay systems...");
            
            // (1) Input
            _world.AddSystem(_inputSystem);
            _world.AddSystem(_inventoryInputSystem);
            _world.AddSystem(_inventoryDragDropSystem);
            
            // (2) Pickup
            _world.AddSystem(new PickupSystem());
            
            // (3) Consumption
            _world.AddSystem(new ConsumptionSystem());
            
            // (4) AI
            _world.AddSystem(new AISystem(_biomeSystem));
            
            // (5) Movement
            _world.AddSystem(new MovementSystem());
            
            // (6) Attachment (weapon visuals)
            _world.AddSystem(new AttachmentSystem());
            _world.AddSystem(new WeaponVisualSystem());
            
            // (7) Bullets
            _bulletSystem = new BulletSystem();
            _bulletSystem.SetMapSize(_chunkedMap.MapWidthInPixels, _chunkedMap.MapHeightInPixels);
            _world.AddSystem(_bulletSystem);
            
            // (8) Block streaming
            _world.AddSystem(new BlockStreamSystem(_blockStreamer));
            
            // (9) Collision
            _world.AddSystem(new CollisionSystem(_spatialIndex));
            
            // (10) Death
            _world.AddSystem(new DeathSystem(_textureManager));
            
            // (11) Construction
            _world.AddSystem(new ConstructionSystem(_worldObjectFactory));
            
            // (12) Spawning systems V2
            _world.AddSystem(new EnemySpawnSystem(
                _regionProvider,
                _enemyFactory,
                baseSpawnInterval: 2f,
                exclusionProvider: _safeZoneManager,
                biomeSystem: _biomeSystem
            ));
            
            _world.AddSystem(new ResourceSpawnSystem(
                _regionProvider,
                _textureManager
            ));
            
            _world.AddSystem(new AppleSpawnSystem(
                new Rectangle(0, 0, _chunkedMap.MapWidthInPixels, _chunkedMap.MapHeightInPixels),
                _textureManager
            ));
            
            // (13) Tree harvesting
            _world.AddSystem(new TreeHarvestSystem(
                _blockStreamer,
                _textureManager,
                _chunkedMap
            ));
            
            // (14) Game state
            _world.AddSystem(_gameStateSystem);
            
            Console.WriteLine("[PlayState] All systems registered");
        }

        private void InitializeBiomeSystem()
        {
            // Build biomes from regions if available
            var biomes = new System.Collections.Generic.List<CubeSurvivor.World.Biomes.Biome>();
            
            foreach (var region in _regionProvider.GetRegions(RegionType.Biome))
            {
                var biomeType = region.Meta.TryGetValue("biomeType", out string typeStr) 
                    ? ParseBiomeType(typeStr) 
                    : CubeSurvivor.World.Biomes.BiomeType.Unknown;
                
                bool allowsEnemySpawns = region.Meta.TryGetValue("allowsEnemySpawns", out string spawnsStr) 
                    ? bool.Parse(spawnsStr) 
                    : true;
                
                int treeDensity = region.Meta.TryGetValue("treeDensity", out string treeDensityStr) 
                    ? int.Parse(treeDensityStr) 
                    : 0;
                
                int goldDensity = region.Meta.TryGetValue("goldDensity", out string goldDensityStr) 
                    ? int.Parse(goldDensityStr) 
                    : 0;
                
                var texture = region.Meta.TryGetValue("texture", out string textureName) 
                    ? _textureManager.GetTexture(Path.GetFileNameWithoutExtension(textureName)) 
                    : null;
                
                biomes.Add(new CubeSurvivor.World.Biomes.Biome(
                    biomeType,
                    region.Area,
                    texture,
                    allowsEnemySpawns,
                    treeDensity,
                    goldDensity
                ));
            }
            
            _biomeSystem = new BiomeSystem(biomes);
            _world.AddSystem(_biomeSystem);
            
            // Tree spawn system
            if (biomes.Count > 0)
            {
                var treeSpawnSystem = new BiomeTreeSpawnSystem(_biomeSystem, _textureManager);
                _world.AddSystem(treeSpawnSystem);
            }
            
            Console.WriteLine($"[PlayStateV2] Biome system initialized with {biomes.Count} biomes");
        }

        private CubeSurvivor.World.Biomes.BiomeType ParseBiomeType(string typeStr)
        {
            return typeStr.ToLowerInvariant() switch
            {
                "forest" => CubeSurvivor.World.Biomes.BiomeType.Forest,
                "cave" => CubeSurvivor.World.Biomes.BiomeType.Cave,
                _ => CubeSurvivor.World.Biomes.BiomeType.Unknown
            };
        }

        private void SpawnPlayer()
        {
            // Get player spawn region
            var playerSpawnRegions = _regionProvider.GetRegions(RegionType.PlayerSpawn).ToList();
            
            Vector2 spawnPosition;
            if (playerSpawnRegions.Count > 0)
            {
                var spawnRegion = playerSpawnRegions[0].Area;
                spawnPosition = new Vector2(
                    spawnRegion.Center.X,
                    spawnRegion.Center.Y
                );
            }
            else
            {
                // Fallback: center of map
                spawnPosition = new Vector2(
                    _chunkedMap.MapWidthInPixels / 2,
                    _chunkedMap.MapHeightInPixels / 2
                );
            }
            
            _playerFactory.CreatePlayer(_world, spawnPosition);
            Console.WriteLine($"[PlayStateV2] Player spawned at {spawnPosition}");
        }

        public void Update(GameTime gameTime)
        {
            // Handle ESC key for pause menu
            KeyboardState keyboardState = Keyboard.GetState();
            bool escPressed = keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape);
            if (escPressed)
            {
                _pauseMenu.Toggle(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
            }

            // Update pause menu
            MouseState mouseState = Mouse.GetState();
            MouseState previousMouseState = new MouseState(); // TODO: track properly if needed
            _pauseMenu.Update(gameTime, mouseState, previousMouseState);
            
            // If paused, don't update game
            if (_pauseMenu.IsOpen)
            {
                _previousKeyboardState = keyboardState;
                return;
            }

            bool inventoryOpen = false;
            var player = _world.GetEntitiesWithComponent<Components.PlayerInputComponent>().FirstOrDefault();
            
            if (player != null)
            {
                var inventoryComp = player.GetComponent<Inventory.Components.InventoryComponent>();
                if (inventoryComp != null && inventoryComp.IsUIOpen)
                {
                    inventoryOpen = true;
                }
            }
            
            bool upgradePending = _world.GetEntitiesWithComponent<Components.UpgradeRequestComponent>().Any();
            
            if (!upgradePending && !inventoryOpen)
            {
                _world.Update(gameTime);
            }
            else if (inventoryOpen)
            {
                _inventoryInputSystem?.Update(gameTime);
                _inventoryDragDropSystem?.Update(gameTime);
            }
            
            if (_gameStateSystem.IsGameOver)
            {
                Console.WriteLine("[PlayState] Game Over - restarting");
                RestartGame();
                _gameStateSystem.Reset();
            }

            if (player != null && !inventoryOpen)
            {
                _cameraService.Update(player);
                _inputSystem?.SetCameraTransform(_cameraService.Transform);
            }
            
            _previousKeyboardState = keyboardState;
        }

        private void RestartGame()
        {
            // Clear all entities
            var allEntities = _world.GetAllEntities().ToList();
            foreach (var entity in allEntities)
            {
                _world.RemoveEntity(entity);
            }
            
            // Clear block streamer
            _blockStreamer.ClearAll();
            
            // Respawn player
            SpawnPlayer();
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.Clear(Color.Black);

            // Draw map (V2 system with chunking)
            _mapRenderSystem.Draw(_cameraService.Transform);

            // Draw entities
            _renderSystem.Draw(_cameraService.Transform);

            // Draw UI
            _uiSystem.Draw();
            _inventoryUISystem?.Draw();
            _consumptionUISystem?.Draw();

            // Draw pause menu on top of everything
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _pauseMenu.Draw(_spriteBatch, _font, _pixelTexture);
            _spriteBatch.End();
        }
    }
}

