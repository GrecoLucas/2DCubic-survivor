using System;
using System.IO;
using System.Linq;
using CubeSurvivor.Core;
using CubeSurvivor.Core.Spatial;
using CubeSurvivor.Entities;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Registries;
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
            
            // Spawn placed items from map (backward compatibility)
            SpawnPlacedItems();
            
            // Spawn items from ItemLayers
            SpawnItemsFromItemLayers();
            
            // Initialize pause menu
            InitializePauseMenu();
            
            _previousKeyboardState = Keyboard.GetState();
            
            Console.WriteLine("[PlayState] Play state initialized successfully");
        }

        private void SpawnPlacedItems()
        {
            if (_chunkedMap?.Definition?.PlacedItems == null) return;

            foreach (var placedItem in _chunkedMap.Definition.PlacedItems)
            {
                // Convert tile coordinates to world position
                Vector2 worldPos = new Vector2(
                    placedItem.Tile.X * _chunkedMap.TileSize + _chunkedMap.TileSize / 2f,
                    placedItem.Tile.Y * _chunkedMap.TileSize + _chunkedMap.TileSize / 2f
                );

                // Spawn item based on ItemId
                SpawnPlacedItem(placedItem, worldPos);
            }

            Console.WriteLine($"[PlayState] Spawned {_chunkedMap.Definition.PlacedItems.Count} placed items");
        }

        private void SpawnItemsFromItemLayers()
        {
            if (_chunkedMap?.Definition?.ItemLayers == null) return;

            int totalSpawned = 0;

            // Spawn from both item layers (ItemsLow and ItemsHigh)
            for (int layerIndex = 0; layerIndex < _chunkedMap.Definition.ItemLayers.Count; layerIndex++)
            {
                var layer = _chunkedMap.Definition.ItemLayers[layerIndex];
                
                // Iterate through all chunks in this layer
                foreach (var chunkKvp in layer.Chunks)
                {
                    var chunkData = chunkKvp.Value;
                    if (chunkData?.Items == null) continue;

                    // Parse chunk coordinate from key (format: "cx,cy")
                    var parts = chunkKvp.Key.Split(',');
                    if (parts.Length != 2) continue;
                    if (!int.TryParse(parts[0], out int cx) || !int.TryParse(parts[1], out int cy))
                        continue;

                    // Calculate chunk world position
                    int chunkWorldX = cx * _chunkedMap.ChunkSize * _chunkedMap.TileSize;
                    int chunkWorldY = cy * _chunkedMap.ChunkSize * _chunkedMap.TileSize;

                    // Iterate through items in chunk
                    for (int ly = 0; ly < _chunkedMap.ChunkSize; ly++)
                    {
                        for (int lx = 0; lx < _chunkedMap.ChunkSize; lx++)
                        {
                            ItemType itemType = chunkData.Items[lx, ly];
                            if (itemType == ItemType.Empty)
                                continue;

                            // Calculate world position (center of tile)
                            int worldX = chunkWorldX + lx * _chunkedMap.TileSize;
                            int worldY = chunkWorldY + ly * _chunkedMap.TileSize;
                            Vector2 worldPos = new Vector2(
                                worldX + _chunkedMap.TileSize / 2f,
                                worldY + _chunkedMap.TileSize / 2f
                            );

                            // Spawn item entity with ItemLayerSourceComponent to track its origin
                            SpawnItemFromType(itemType, worldPos, layerIndex, lx + cx * _chunkedMap.ChunkSize, ly + cy * _chunkedMap.ChunkSize);
                            totalSpawned++;
                        }
                    }
                }
            }

            Console.WriteLine($"[PlayState] Spawned {totalSpawned} items from ItemLayers");
        }

        private void SpawnItemFromType(ItemType itemType, Vector2 worldPos, int layerIndex = -1, int tileX = -1, int tileY = -1)
        {
            Entity itemEntity = null;
            
            switch (itemType)
            {
                case ItemType.Hammer:
                    var hammerFactory = new Entities.HammerEntityFactory();
                    hammerFactory.SetTextureManager(_textureManager);
                    itemEntity = hammerFactory.CreateHammer(_world, worldPos);
                    break;
                case ItemType.Apple:
                    var appleFactory = new Entities.AppleEntityFactory();
                    appleFactory.SetTextureManager(_textureManager);
                    itemEntity = appleFactory.CreateApple(_world, worldPos);
                    break;
                case ItemType.WoodPickup:
                    var woodFactory = new Entities.WoodEntityFactory();
                    woodFactory.SetTextureManager(_textureManager);
                    itemEntity = woodFactory.CreateWood(_world, worldPos);
                    break;
                case ItemType.GoldPickup:
                    var goldFactory = new Entities.GoldEntityFactory();
                    goldFactory.SetTextureManager(_textureManager);
                    itemEntity = goldFactory.CreateGold(_world, worldPos, 1);
                    break;
                case ItemType.Brain:
                    var brainFactory = new Entities.BrainEntityFactory();
                    brainFactory.SetTextureManager(_textureManager);
                    itemEntity = brainFactory.CreateBrain(_world, worldPos);
                    break;
                case ItemType.Gun:
                    // Gun é uma arma, não um item de pickup normal - pode ficar como está ou implementar depois
                    Console.WriteLine($"[PlayState] Gun item spawn not yet implemented");
                    break;
                default:
                    Console.WriteLine($"[PlayState] Unknown item type: {itemType}");
                    break;
            }
            
            // If item came from ItemLayer, mark it so we can remove it from the layer when collected
            if (itemEntity != null && layerIndex >= 0 && tileX >= 0 && tileY >= 0)
            {
                itemEntity.AddComponent(new Components.ItemLayerSourceComponent(layerIndex, tileX, tileY));
            }
        }

        private void SpawnPlacedItem(PlacedItemDefinition placedItem, Vector2 worldPos)
        {
            // Use appropriate factory based on ItemId
            // This is a simple implementation - can be extended with a factory registry
            switch (placedItem.ItemId.ToLower())
            {
                case "hammer":
                    var hammerFactory = new Entities.HammerEntityFactory();
                    hammerFactory.SetTextureManager(_textureManager);
                    hammerFactory.CreateHammer(_world, worldPos);
                    break;
                case "apple":
                    var appleFactory = new Entities.AppleEntityFactory();
                    appleFactory.SetTextureManager(_textureManager);
                    appleFactory.CreateApple(_world, worldPos);
                    break;
                case "wood":
                    var woodFactory = new Entities.WoodEntityFactory();
                    woodFactory.SetTextureManager(_textureManager);
                    woodFactory.CreateWood(_world, worldPos);
                    break;
                case "gold":
                    var goldFactory = new Entities.GoldEntityFactory();
                    goldFactory.SetTextureManager(_textureManager);
                    goldFactory.CreateGold(_world, worldPos, placedItem.Amount);
                    break;
                default:
                    Console.WriteLine($"[PlayState] Unknown item ID: {placedItem.ItemId}");
                    break;
            }
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
            
            _enemyFactory = new EnemyFactory(_textureManager); // Pass TextureManager for texture loading
            _bulletFactory = new BulletFactory();
            _worldObjectFactory = new WorldObjectFactory();
            
            // Load textures
            LoadTextures();
        }

        private void LoadTextures()
        {
            try
            {
                // Base textures
                _textureManager.LoadTexture("grass", "grass.png");
                _textureManager.LoadTexture("cave", "cave.png");
                _textureManager.LoadTexture("player", "player.png");
                _textureManager.LoadTexture("wall", "wall.png");
                _textureManager.LoadTexture("crate", "crate.png");
                
                // Variant textures for trees and stones
                _textureManager.LoadTexture("tree1", "tree1.png");
                _textureManager.LoadTexture("tree2", "tree2.png");
                _textureManager.LoadTexture("stone1", "stone1.png");
                _textureManager.LoadTexture("stone2", "stone2.png");
                
                // Enemy animation frames
                _textureManager.LoadTexture("glorb1", "glorb1.png");
                _textureManager.LoadTexture("glorb2", "glorb2.png");
                _textureManager.LoadTexture("glorb3", "glorb3.png");
                
                // Items
                _textureManager.LoadTexture("apple", "apple.png");
                _textureManager.LoadTexture("brain", "brain.png");
                _textureManager.LoadTexture("gun", "gun.png");
                _textureManager.LoadTexture("hammer", "hammer.png");
                _textureManager.LoadTexture("wood", "wood.png");
                _textureManager.LoadTexture("gold", "gold.png");
                
                Console.WriteLine("[PlayState] Textures loaded (including variants)");
                
                // Debug: List all loaded texture keys
                Console.WriteLine("[PlayState] Loaded texture keys:");
                var textureKeys = new[] { "grass", "cave", "player", "wall", "crate", "tree1", "tree2", "stone1", "stone2", 
                                          "glorb1", "glorb2", "glorb3", "apple", "brain", "gun", "hammer", "wood", "gold" };
                foreach (var key in textureKeys)
                {
                    var tex = _textureManager.GetTexture(key);
                    Console.WriteLine($"  {key}: {(tex != null ? "✓" : "✗ MISSING")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayState] Error loading textures: {ex.Message}");
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
            // Initialize variant system for deterministic tile/block variants
            Console.WriteLine("[PlayState] Initializing TileVisualCatalog...");
            var catalog = new TileVisualCatalog(_textureManager);
            catalog.InitializeDefaults();
            var variantResolver = new VariantResolver(catalog);
            // Use a deterministic seed based on map dimensions for consistency
            int worldSeed = _chunkedMap.Definition != null 
                ? (_chunkedMap.Definition.MapWidth * 73856093) ^ (_chunkedMap.Definition.MapHeight * 19349663)
                : 0;
            Console.WriteLine($"[PlayState] VariantResolver initialized with worldSeed={worldSeed}");
            
            // Debug: List enemy registry entries
            Console.WriteLine("[PlayState] Enemy registry entries:");
            foreach (var key in EnemyRegistry.Instance.GetAllKeys())
            {
                var def = EnemyRegistry.Instance.Get(key);
                Console.WriteLine($"  {key}: textureName={def.TextureName ?? "null"}, frames={(def.TextureName != null && def.TextureName.EndsWith("1") ? "3 (detected)" : "1")}");
            }
            
            // Update WorldObjectFactory with variant resolver
            if (_worldObjectFactory is WorldObjectFactory wof)
            {
                // Create new factory with variant resolver
                _worldObjectFactory = new WorldObjectFactory(_textureManager, catalog, variantResolver, worldSeed);
            }
            
            // Initialize map rendering with variant resolver
            _mapRenderSystem = new MapRenderSystem(
                _chunkedMap,
                _spriteBatch,
                _cameraService,
                _textureManager,
                variantResolver,
                worldSeed
            );
            
            // Initialize block streaming with variant resolver
            _blockStreamer = new BlockEntityStreamer(
                _chunkedMap,
                _worldObjectFactory,
                _world,
                _cameraService,
                _textureManager,
                catalog,
                variantResolver,
                worldSeed
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
            
            // (2) Pickup (pass map so it can remove items from ItemLayers)
            _world.AddSystem(new PickupSystem(_chunkedMap));
            
            // (3) Consumption
            _world.AddSystem(new ConsumptionSystem());
            
            // (4) Animation (before AI so facing is updated)
            _world.AddSystem(new SpriteAnimationSystem());
            
            // (5) AI
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
            var deathSystem = new DeathSystem(_textureManager);
            deathSystem.SetBlockStreamer(_blockStreamer); // Allow death system to remove blocks from map
            _world.AddSystem(deathSystem);
            
            // (11) Construction
            _world.AddSystem(new ConstructionSystem(_worldObjectFactory));
            
            // (12) Spawning systems V2
            // EXTENSIVE DEBUG LOG: Region loading
            Console.WriteLine($"[PlayState] === Region Loading ===");
            Console.WriteLine($"[PlayState] Total regions: {_chunkedMap.Definition.Regions?.Count ?? 0}");
            
            var enemyRegions = _regionProvider.GetRegions(RegionType.EnemySpawn).ToList();
            Console.WriteLine($"[PlayState] EnemySpawn regions found: {enemyRegions.Count}");
            foreach (var region in enemyRegions)
            {
                Console.WriteLine($"[PlayState]   - {region.Id}: Type={region.Type}({(int)region.Type}) AreaTiles L={region.Area.Left} R={region.Area.Right} T={region.Area.Top} B={region.Area.Bottom}");
                if (region.Meta != null && region.Meta.Count > 0)
                {
                    var metaStr = string.Join(", ", region.Meta.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    Console.WriteLine($"[PlayState]     meta: {metaStr}");
                }
                else
                {
                    Console.WriteLine($"[PlayState]     meta: (empty)");
                }
            }
            
            // Log other region types too
            foreach (RegionType type in System.Enum.GetValues(typeof(RegionType)))
            {
                if (type == RegionType.EnemySpawn) continue;
                var regions = _regionProvider.GetRegions(type).ToList();
                if (regions.Count > 0)
                {
                    Console.WriteLine($"[PlayState] {type} regions: {regions.Count}");
                }
            }

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
                var spawnRegion = playerSpawnRegions[0];
                // Convert tile coordinates to world pixels
                Rectangle worldArea = spawnRegion.ToWorldPixels(_chunkedMap.TileSize);
                spawnPosition = new Vector2(
                    worldArea.Center.X,
                    worldArea.Center.Y
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

