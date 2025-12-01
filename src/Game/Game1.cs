using CubeSurvivor.Core;
using CubeSurvivor.Core.Spatial;
using CubeSurvivor.Entities;
using CubeSurvivor.Systems;
using CubeSurvivor.Inventory.Systems;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Entities.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;
using System.IO;

namespace CubeSurvivor
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        private readonly GameWorld _world;
        private RenderSystem _renderSystem;
        private UISystem _uiSystem;
        private GameStateSystem _gameStateSystem;
        private PlayerInputSystem _inputSystem;
        private InventoryInputSystem _inventoryInputSystem;
        private InventoryUISystem _inventoryUISystem;
        private InventoryDragDropSystem _inventoryDragDropSystem;
        private CubeSurvivor.Systems.World.BiomeSystem _biomeSystem;
        private EnemySpawnSystem _enemySpawnSystem;
        private ConsumptionUISystem _consumptionUISystem;

        private readonly IPlayerFactory _playerFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly IBulletFactory _bulletFactory;
        private readonly IWorldObjectFactory _worldObjectFactory;

        private readonly CameraService _cameraService;

        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private Texture2D _grassTexture;
        private Texture2D _caveTexture;
        
        private TextureManager _textureManager;
        private SafeZoneManager _safeZoneManager;
        private LevelDefinition _levelDefinition;
        private ISpatialIndex _spatialIndex;
        private WorldBackgroundRenderer _backgroundRenderer;
        private BulletSystem _bulletSystem;
        
        // Sistema de áreas e portais
        private WorldMapDefinition _worldMap;
        private AreaManager _areaManager;

        public Game1()
        {
            Console.WriteLine("[Game1] Construtor iniciado");
            _graphics = new GraphicsDeviceManager(this);
            _world = new GameWorld();
            
            Console.WriteLine("[Game1] Criando factories...");
            _playerFactory = new PlayerFactory();
            _enemyFactory = new EnemyFactory();
            _bulletFactory = new BulletFactory();
            _worldObjectFactory = new WorldObjectFactory();
            var weaponVisualFactory = new WeaponVisualFactory();
            
            // Configure PlayerFactory with weapon visual factory
            if (_playerFactory is PlayerFactory pf)
            {
                pf.SetWeaponVisualFactory(weaponVisualFactory);
            }
            
            Console.WriteLine("[Game1] Criando serviço de câmera...");
            _cameraService = new CameraService(
                GameConfig.ScreenWidth, 
                GameConfig.ScreenHeight, 
                GameConfig.MapWidth, 
                GameConfig.MapHeight
            );
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GameConfig.ScreenWidth;
            _graphics.PreferredBackBufferHeight = GameConfig.ScreenHeight;
            
            Console.WriteLine("[Game1] Construtor concluído");
        }

        protected override void Initialize()
        {
            Console.WriteLine("[Game1] Initialize() iniciado");
            
            // Permitir redimensionamento da janela (habilita o botão de maximizar)
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;

            base.Initialize();
            Console.WriteLine("[Game1] Initialize() concluído");
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            if (Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0)
            {
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                _graphics.ApplyChanges();

                UpdateScreenSize(Window.ClientBounds.Width, Window.ClientBounds.Height);
            }
        }

        private void UpdateScreenSize(int width, int height)
        {
             _cameraService.UpdateScreenSize(width, height);
             _inputSystem.SetScreenSize(width, height);
             _uiSystem.SetScreenSize(width, height);
             _inventoryUISystem.SetScreenSize(width, height);
             _consumptionUISystem.SetScreenSize(width, height);
             _backgroundRenderer?.UpdateScreenSize(width, height);
             Console.WriteLine($"[Game1] Screen size updated to {width}x{height}");
        }

        protected override void LoadContent()
        {
            Console.WriteLine("[Game1] LoadContent() iniciado");
            
            try
            {
                Console.WriteLine("[Game1] Criando SpriteBatch...");
                _spriteBatch = new SpriteBatch(GraphicsDevice);

                Console.WriteLine("[Game1] Criando textura de pixel...");
                _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
                
                Console.WriteLine("[Game1] Inicializando TextureManager...");
                _textureManager = new TextureManager(GraphicsDevice);

                Console.WriteLine("[Game1] Inicializando SpatialHashGrid...");
                _spatialIndex = new SpatialHashGrid(cellSize: 128);

                Console.WriteLine("[Game1] Inicializando SafeZoneManager...");
                _safeZoneManager = new SafeZoneManager(_worldObjectFactory);

                Console.WriteLine("[Game1] Criando definição de nível (carrega JSON)...");
                CreateLevelDefinition();

                Console.WriteLine("[Game1] Carregando texturas automaticamente de assets/textures...");
                try
                {
                    _textureManager.LoadAllFromDirectory("textures");
                    
                    // Recuperar referências para campos específicos usados em fallbacks
                    _grassTexture = _textureManager.GetTexture("grass");
                    _caveTexture = _textureManager.GetTexture("cave");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao carregar texturas: " + ex.Message);
                }
                
                Console.WriteLine("[Game1] Configurando factories com texturas...");
                if (_playerFactory is PlayerFactory playerFactory)
                {
                    playerFactory.SetTextureManager(_textureManager);
                }

                Console.WriteLine("[Game1] Inicializando WorldBackgroundRenderer (data-driven biomes)...");
                _backgroundRenderer = new WorldBackgroundRenderer(
                    GameConfig.MapWidth,
                    GameConfig.MapHeight,
                    tileSize: 128,
                    screenWidth: GameConfig.ScreenWidth,
                    screenHeight: GameConfig.ScreenHeight
                );

                // Configurar BiomeSystem (ECS-friendly, open for extension)
                Console.WriteLine("[Game1] Configurando BiomeSystem...");
                var biomes = new System.Collections.Generic.List<CubeSurvivor.World.Biomes.Biome>();

                // Sem fallback legado de metades; requer definição JSON para controle total.

                _biomeSystem = new CubeSurvivor.Systems.World.BiomeSystem(biomes);
                _world.AddSystem(_biomeSystem);

                // Informar o renderer sobre o provider de textura por posição (consulta ao BiomeSystem)
                _backgroundRenderer.SetBiomeTextureProvider(pos => _biomeSystem.GetTextureForPosition(pos));

                Console.WriteLine("[Game1] Carregando fonte DefaultFont...");
                try
                {
                    _font = Content.Load<SpriteFont>("DefaultFont");
                    Console.WriteLine("[Game1] Fonte carregada com sucesso!");
                }
                catch (Exception)
                {
                    _font = null;
                    Console.WriteLine("[Game1] Não foi possível carregar DefaultFont");
                    Console.WriteLine("[Game1] Continuando sem fonte, a UI desenhará apenas barras visuais.");
                }

                Console.WriteLine("[Game1] Configurando sistemas...");
                _renderSystem = new RenderSystem(_spriteBatch);
                _renderSystem.Initialize(_world);
                _renderSystem.CreatePixelTexture(GraphicsDevice);

                // Criar MainMenu e subescrever o comportamento para iniciar o jogo quando Play for clicado
                var mainMenu = new MainMenu();
                mainMenu.OnPlayRequested += () =>
                {
                    Console.WriteLine("[MainMenu] Play solicitado, iniciando jogo...");
                    InitializeGame();
                };

                _uiSystem = new UISystem(_spriteBatch, _font, _pixelTexture, mainMenu, _textureManager.GetTexture("brain"));
                _uiSystem.Initialize(_world);

                _gameStateSystem = new GameStateSystem();
                _gameStateSystem.Initialize(_world);
                _gameStateSystem.OnGameOver += () => { Console.WriteLine("[GameState] GameOver disparado!"); };

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
                
                Console.WriteLine("[Game1] Adicionando sistemas ao mundo...");
                _world.AddSystem(_inputSystem);
                _world.AddSystem(_inventoryInputSystem);
                _world.AddSystem(_inventoryDragDropSystem);
                _world.AddSystem(new PickupSystem());
                _world.AddSystem(new ConsumptionSystem());
                _world.AddSystem(new AISystem());
                _world.AddSystem(new MovementSystem(_biomeSystem));
                
                // AttachmentSystem must run after movement/input to update attached items
                _world.AddSystem(new AttachmentSystem());
                
                // WeaponVisualSystem controls visibility of weapon visuals based on equipped items
                _world.AddSystem(new WeaponVisualSystem());
                
                _bulletSystem = new BulletSystem();
                _world.AddSystem(_bulletSystem);
                
                _world.AddSystem(new CollisionSystem(_spatialIndex));
                _world.AddSystem(new DeathSystem(_textureManager));
                _world.AddSystem(_gameStateSystem);
                
                // Sistema de construção
                _world.AddSystem(new ConstructionSystem(_worldObjectFactory));
                
                // Sistema de teleporte por portais
                _world.AddSystem(new PortalTeleportSystem(OnAreaChange));
                Console.WriteLine("[Game1] PortalTeleportSystem added");

                Rectangle spawnArea = new Rectangle(0, 0, GameConfig.MapWidth, GameConfig.MapHeight);
                _world.AddSystem(new AppleSpawnSystem(spawnArea, _textureManager));
                
                Console.WriteLine("[Game1] LoadContent() concluído com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Game1] ERRO em LoadContent: " + ex.Message);
                Console.WriteLine("Stack: " + ex.StackTrace);
                throw;
            }
        }

        private void CreateLevelDefinition()
        {
            // Carregar world map definition (áreas conectadas por portais)
            string worldMapPath = Path.Combine("assets", "maps.txt");
            _worldMap = WorldMapLoader.Load(worldMapPath);
            
            Console.WriteLine($"[Game1] World map loaded with {_worldMap.Areas.Count} areas");
            
            // Criar area manager
            _areaManager = new AreaManager(_worldMap, _world);
            
            // Carregar área inicial
            var startAreaId = _worldMap.StartAreaId;
            var mapDef = _areaManager.LoadArea(startAreaId);
            
            if (mapDef == null)
            {
                Console.WriteLine("[Game1] ⚠ Failed to load starting area, creating empty map");
                _levelDefinition = new LevelDefinition();
            }
            else
            {
                // Converter MapDefinition para LevelDefinition
                _levelDefinition = mapDef.ToLevelDefinition();
                Console.WriteLine($"[Game1] Starting area {startAreaId} loaded: {_levelDefinition.SafeZones.Count} zone(s), {_levelDefinition.Crates.Count} crate(s)");
                Console.WriteLine($"[Game1] Current Area: {startAreaId}");
            }
        }

        private void InitializeGame()
        {
            Console.WriteLine("[Game1] InitializeGame() iniciado");
            
            // Obter dimensões do mapa (do JSON ou usar padrão)
            int mapWidth = _levelDefinition?.MapWidth ?? GameConfig.MapWidth;
            int mapHeight = _levelDefinition?.MapHeight ?? GameConfig.MapHeight;

            // Atualizar todos os sistemas com as dimensões corretas do mapa
            _cameraService.SetMapSize(mapWidth, mapHeight);
            _backgroundRenderer?.SetMapSize(mapWidth, mapHeight);
            _bulletSystem?.SetMapSize(mapWidth, mapHeight);
            Console.WriteLine($"[Game1] Sistemas configurados para mapa {mapWidth}x{mapHeight}");
            
            // Inicializar zonas seguras (criar paredes)
            if (_levelDefinition != null)
            {
                Console.WriteLine("[Game1] Inicializando zonas seguras...");
                _safeZoneManager.InitializeZones(_world, _levelDefinition);

                // Criar caixas/obstáculos definidos no JSON
                if (_levelDefinition.Crates.Count > 0)
                {
                    Console.WriteLine($"[Game1] Criando {_levelDefinition.Crates.Count} caixas...");
                    foreach (var crateDef in _levelDefinition.Crates)
                    {
                        _worldObjectFactory.CreateCrate(_world, crateDef.Position, crateDef.IsDestructible, crateDef.MaxHealth);
                    }
                }

                // Criar pickups (wood, hammer, etc.) definidos no JSON
                if (_levelDefinition.Pickups.Count > 0)
                {
                    Console.WriteLine($"[Game1] Criando {_levelDefinition.Pickups.Count} pickups...");
                    SpawnPickups();
                }

                // Adicionar ResourceSpawnSystem depois de carregar o levelDefinition
                _world.AddSystem(new ResourceSpawnSystem(_levelDefinition, _textureManager));
                Console.WriteLine("[Game1] ResourceSpawnSystem adicionado");

                // Configurar biomas e spawn de inimigos
                SetupBiomesAndEnemies();
            }
            
            // Criar jogador mais para cima/esquerda, próximo das safe zones
            Vector2 playerStartPosition = new Vector2(1000, 750);
            _playerFactory.CreatePlayer(_world, playerStartPosition);
            Console.WriteLine("[Game1] Jogador criado em " + playerStartPosition);
        }

        private void SpawnPickups()
        {
            var woodFactory = new WoodEntityFactory();
            var hammerFactory = new HammerEntityFactory();
            
            woodFactory.SetTextureManager(_textureManager);
            hammerFactory.SetTextureManager(_textureManager);

            foreach (var pickup in _levelDefinition.Pickups)
            {
                var pickupType = pickup.Type.ToLowerInvariant();
                
                if (pickupType == "wood")
                {
                    woodFactory.CreateWood(_world, pickup.Position, (int)pickup.Amount);
                }
                else if (pickupType == "hammer")
                {
                    hammerFactory.CreateHammer(_world, pickup.Position);
                }
            }
        }

        private void RestartGame()
        {
            var allEntities = _world.GetAllEntities().ToList();
            foreach (var entity in allEntities)
            {
                _world.RemoveEntity(entity);
            }

            InitializeGame();
        }
        
        /// <summary>
        /// Chamado quando o player entra em um portal para mudar de área
        /// </summary>
        private void OnAreaChange(string newAreaId, Vector2 relativeSpawnPosition)
        {
            Console.WriteLine($"[Game1] === Area Change: {_areaManager.CurrentAreaId} -> {newAreaId} ===");
            
            // Limpar área atual (mantém player)
            _areaManager.ClearCurrentArea();
            
            // Carregar nova área
            var mapDef = _areaManager.LoadArea(newAreaId);
            if (mapDef == null)
            {
                Console.WriteLine($"[Game1] Failed to load area {newAreaId}!");
                return;
            }
            
            // Atualizar LevelDefinition
            _levelDefinition = mapDef.ToLevelDefinition();
            
            // Calcular posição de spawn
            Vector2 spawnPosition = _areaManager.CalculateSpawnPosition(relativeSpawnPosition);
            
            // Mover player para nova posição
            var player = _world.GetEntitiesWithComponent<Components.PlayerInputComponent>().FirstOrDefault();
            if (player != null)
            {
                var transform = player.GetComponent<Components.TransformComponent>();
                if (transform != null)
                {
                    transform.Position = spawnPosition;
                    Console.WriteLine($"[Game1] Player spawned at {spawnPosition} in area {newAreaId}");
                }
            }
            
            // Atualizar dimensões do mapa
            int mapWidth = _levelDefinition?.MapWidth ?? GameConfig.MapWidth;
            int mapHeight = _levelDefinition?.MapHeight ?? GameConfig.MapHeight;
            
            _cameraService.SetMapSize(mapWidth, mapHeight);
            _backgroundRenderer?.SetMapSize(mapWidth, mapHeight);
            _bulletSystem?.SetMapSize(mapWidth, mapHeight);
            
            Console.WriteLine($"[Game1] Map systems updated for {mapWidth}x{mapHeight}");
            
            // Reinicializar elementos da área
            if (_levelDefinition != null)
            {
                // Criar safe zones
                if (_levelDefinition.SafeZones.Count > 0)
                {
                    _safeZoneManager.InitializeZones(_world, _levelDefinition);
                    Console.WriteLine($"[Game1] {_levelDefinition.SafeZones.Count} safe zone(s) initialized");
                }
                
                // Criar crates
                if (_levelDefinition.Crates.Count > 0)
                {
                    foreach (var crateDef in _levelDefinition.Crates)
                    {
                        _worldObjectFactory.CreateCrate(_world, crateDef.Position, crateDef.IsDestructible, crateDef.MaxHealth);
                    }
                    Console.WriteLine($"[Game1] {_levelDefinition.Crates.Count} crate(s) created");
                }
                
                // Criar pickups
                if (_levelDefinition.Pickups.Count > 0)
                {
                    SpawnPickups();
                    Console.WriteLine($"[Game1] {_levelDefinition.Pickups.Count} pickup(s) spawned");
                }

                // Reconfigurar biomas e inimigos para a nova área
                SetupBiomesAndEnemies();
            }
            
            Console.WriteLine($"[Game1] Now in area: {newAreaId}");
        }

        private void SetupBiomesAndEnemies()
        {
            if (_levelDefinition == null) return;

            // 1. Atualizar BiomeSystem
            _biomeSystem.Clear();
            
            foreach (var bdef in _levelDefinition.Biomes)
            {
                Microsoft.Xna.Framework.Graphics.Texture2D tex = null;
                if (!string.IsNullOrWhiteSpace(bdef.TextureKey))
                {
                    tex = _textureManager.GetTexture(bdef.TextureKey) ?? _textureManager.LoadTexture(bdef.TextureKey, bdef.TextureKey);
                }
                
                // Se ainda null, fallback
                if (tex == null)
                {
                     tex = _textureManager.GetTexture("floor");
                }

                _biomeSystem.RegisterBiome(new CubeSurvivor.World.Biomes.Biome(
                    bdef.Type,
                    bdef.Area,
                    tex,
                    allowsEnemySpawns: bdef.AllowsEnemySpawns,
                    treeDensity: bdef.TreeDensity,
                    allowedEnemies: bdef.AllowedEnemies,
                    isWalkable: bdef.IsWalkable
                ));
            }
            Console.WriteLine($"[Game1] BiomeSystem updated with {_levelDefinition.Biomes.Count} biomes");

            // 2. Configurar EnemySpawnSystem
            // Remover sistema antigo se existir
            if (_enemySpawnSystem != null)
            {
                _world.RemoveSystem(_enemySpawnSystem);
            }

            // Criar novo sistema
            int mapWidth = _levelDefinition.MapWidth ?? GameConfig.MapWidth;
            int mapHeight = _levelDefinition.MapHeight ?? GameConfig.MapHeight;
            var spawnArea = new Rectangle(0, 0, mapWidth, mapHeight);
            
            _enemySpawnSystem = new EnemySpawnSystem(
                spawnArea, 
                _enemyFactory, 
                spawnInterval: 2f, 
                maxEnemies: 50, 
                exclusionProvider: _safeZoneManager, 
                biomeSystem: _biomeSystem
            );
            _world.AddSystem(_enemySpawnSystem);
            Console.WriteLine("[Game1] EnemySpawnSystem configured and added");
        }

        protected override void Update(GameTime gameTime)
        {
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
                Console.WriteLine("[Game1] GameOver detectado, reiniciando...");
                RestartGame();
                _gameStateSystem.Reset();
            }

            if (player != null && !inventoryOpen)
            {
                _cameraService.Update(player);
                _inputSystem?.SetCameraTransform(_cameraService.Transform);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Desenhar fundo do mapa usando camera culling (muito mais eficiente)
            if (_backgroundRenderer != null)
            {
                _backgroundRenderer.Draw(_spriteBatch, _cameraService.Transform);
            }

            _renderSystem.Draw(_cameraService.Transform);

            _uiSystem.Draw();
            
            _inventoryUISystem?.Draw();
            
            _consumptionUISystem?.Draw();

            base.Draw(gameTime);
        }
    }
}
