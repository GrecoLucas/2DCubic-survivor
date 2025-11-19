using CubeSurvivor.Core;
using CubeSurvivor.Core.Spatial;
using CubeSurvivor.Entities;
using CubeSurvivor.Systems;
using CubeSurvivor.Inventory.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;
using System.IO;

namespace CubeSurvivor
{
    public class Game1 : Game
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
        private ConsumptionUISystem _consumptionUISystem;

        private readonly IPlayerFactory _playerFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly IBulletFactory _bulletFactory;
        private readonly IWorldObjectFactory _worldObjectFactory;

        private readonly CameraService _cameraService;

        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private Texture2D _floorTexture;
        
        private TextureManager _textureManager;
        private SafeZoneManager _safeZoneManager;
        private LevelDefinition _levelDefinition;
        private ISpatialIndex _spatialIndex;
        private WorldBackgroundRenderer _backgroundRenderer;
        private BulletSystem _bulletSystem;

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
            base.Initialize();
            Console.WriteLine("[Game1] Initialize() concluído");
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

                Console.WriteLine("[Game1] Carregando texturas do jogo...");
                try
                {
                    _floorTexture = _textureManager.LoadTexture("floor", "floor.png");
                    if (_floorTexture == null)
                    {
                        Console.WriteLine("  Textura do piso não encontrada, usando cor sólida");
                    }
                    
                    _textureManager.LoadTexture("player", "player.png");
                    _textureManager.LoadTexture("apple", "apple.png");
                    _textureManager.LoadTexture("brain", "brain.png");
                    _textureManager.LoadTexture("gun", "gun.png");
                    _textureManager.LoadTexture("hammer", "hammer.png");
                    _textureManager.LoadTexture("wood", "wood.png");
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

                Console.WriteLine("[Game1] Inicializando WorldBackgroundRenderer...");
                if (_floorTexture != null)
                {
                    _backgroundRenderer = new WorldBackgroundRenderer(
                        _floorTexture,
                        GameConfig.MapWidth,
                        GameConfig.MapHeight,
                        tileSize: 128,
                        screenWidth: GameConfig.ScreenWidth,
                        screenHeight: GameConfig.ScreenHeight
                    );
                }

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
                _world.AddSystem(new MovementSystem());
                
                _bulletSystem = new BulletSystem();
                _world.AddSystem(_bulletSystem);
                
                _world.AddSystem(new CollisionSystem(_spatialIndex));
                _world.AddSystem(new DeathSystem(_textureManager));
                _world.AddSystem(_gameStateSystem);
                
                // Sistema de construção
                _world.AddSystem(new ConstructionSystem(_worldObjectFactory));

                Rectangle spawnArea = new Rectangle(0, 0, GameConfig.MapWidth, GameConfig.MapHeight);
                _world.AddSystem(new EnemySpawnSystem(spawnArea, _enemyFactory, GameConfig.EnemySpawnInterval, GameConfig.MaxEnemies, _safeZoneManager));
                _world.AddSystem(new AppleSpawnSystem(spawnArea, _textureManager));

                Console.WriteLine("[Game1] Criando definição de nível...");
                CreateLevelDefinition();
                
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
            // Carregar mundo a partir de JSON (data-driven)
            string worldPath = Path.Combine("assets", "world1.json");
            _levelDefinition = WorldDefinitionLoader.LoadFromJson(worldPath);

            if (_levelDefinition == null)
            {
                Console.WriteLine("[Game1] ⚠ Falha ao carregar mundo, usando mapa vazio");
                _levelDefinition = new LevelDefinition();
            }

            Console.WriteLine($"[Game1] Nível carregado: {_levelDefinition.SafeZones.Count} zona(s), {_levelDefinition.Crates.Count} caixa(s)");
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
