using CubeSurvivor.Core;
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
    /// <summary>
    /// Classe principal do jogo
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Sistema ECS
        private readonly GameWorld _world;
        private RenderSystem _renderSystem;
        private UISystem _uiSystem;
        private GameStateSystem _gameStateSystem;
        private PlayerInputSystem _inputSystem;
        private InventoryInputSystem _inventoryInputSystem;
        private InventoryUISystem _inventoryUISystem;

        // Factories
        private readonly IPlayerFactory _playerFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly IBulletFactory _bulletFactory;

        // Serviço de câmera
        private readonly CameraService _cameraService;

        // Font para UI (vamos criar uma simples)
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private Texture2D _floorTexture;
public Game1()
        {
            Console.WriteLine("[Game1] Construtor iniciado");
            _graphics = new GraphicsDeviceManager(this);
            _world = new GameWorld();
            
            Console.WriteLine("[Game1] Criando factories...");
            _playerFactory = new PlayerFactory();
            _enemyFactory = new EnemyFactory();
            _bulletFactory = new BulletFactory();
            
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

                Console.WriteLine("[Game1] Tentando carregar textura do piso...");
                try
                {
                    string[] candidates = new[]
                    {
                        Path.Combine("assets", "floor.png"),
                        Path.Combine(Content.RootDirectory ?? "Content", "..", "assets", "floor.png"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "floor.png"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\assets\\floor.png")
                    };

                    foreach (var path in candidates)
                    {
                        Console.WriteLine($"  Tentando: {path}");
                        if (File.Exists(path))
                        {
                            Console.WriteLine($"  ✓ Arquivo encontrado!");
                            using (var stream = File.OpenRead(path))
                            {
                                _floorTexture = Texture2D.FromStream(GraphicsDevice, stream);
                            }
                            break;
                        }
                    }
                    
                    if (_floorTexture == null)
                    {
                        Console.WriteLine("  ⚠ Nenhuma textura encontrada, usando cor sólida");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠ Erro ao carregar textura: {ex.Message}");
                    _floorTexture = null;
                }

                Console.WriteLine("[Game1] Carregando fonte DefaultFont...");
                try
                {
                    _font = Content.Load<SpriteFont>("DefaultFont");
                    Console.WriteLine("[Game1] Fonte carregada com sucesso!");
                }
                catch (Exception ex)
                {
                    _font = null;
                    Console.WriteLine($"[Game1] Não foi possível carregar DefaultFont: {ex.GetType().Name} - {ex.Message}");
                    Console.WriteLine("[Game1] Continuando sem fonte, a UI desenhará apenas barras visuais.");
                }

                Console.WriteLine("[Game1] Configurando sistemas...");
                _renderSystem = new RenderSystem(_spriteBatch);
                _renderSystem.Initialize(_world);
                _renderSystem.CreatePixelTexture(GraphicsDevice);

                _uiSystem = new UISystem(_spriteBatch, _font, _pixelTexture);
                _uiSystem.Initialize(_world);

                _gameStateSystem = new GameStateSystem();
                _gameStateSystem.Initialize(_world);
                _gameStateSystem.OnGameOver += () => { Console.WriteLine("[GameState] GameOver disparado!"); };

                _inputSystem = new PlayerInputSystem(_bulletFactory);
                _inputSystem.SetScreenSize(GameConfig.ScreenWidth, GameConfig.ScreenHeight);
                
                // Sistemas de inventário
                _inventoryInputSystem = new InventoryInputSystem();
                _inventoryInputSystem.Initialize(_world);
                
                _inventoryUISystem = new InventoryUISystem(_spriteBatch, _font, _pixelTexture);
                _inventoryUISystem.Initialize(_world);
                _inventoryUISystem.SetScreenSize(GameConfig.ScreenWidth, GameConfig.ScreenHeight);
                
                Console.WriteLine("[Game1] Adicionando sistemas ao mundo...");
                _world.AddSystem(_inputSystem);
                _world.AddSystem(_inventoryInputSystem);
                _world.AddSystem(new PickupSystem()); // Sistema de coleta de itens
                _world.AddSystem(new ConsumptionSystem()); // Sistema de consumo de itens
                _world.AddSystem(new AISystem());
                _world.AddSystem(new MovementSystem());
                _world.AddSystem(new BulletSystem());
                _world.AddSystem(new CollisionSystem());
                _world.AddSystem(new DeathSystem());
                _world.AddSystem(_gameStateSystem);

                Rectangle spawnArea = new Rectangle(0, 0, GameConfig.MapWidth, GameConfig.MapHeight);
                _world.AddSystem(new EnemySpawnSystem(spawnArea, _enemyFactory, GameConfig.EnemySpawnInterval, GameConfig.MaxEnemies));
                _world.AddSystem(new AppleSpawnSystem(spawnArea)); // Sistema de spawn de maçãs

                Console.WriteLine("[Game1] Criando jogador...");
                InitializeGame();
                
                Console.WriteLine("[Game1] LoadContent() concluído com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Game1] ERRO em LoadContent: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }

        private void InitializeGame()
        {
            Console.WriteLine("[Game1] InitializeGame() iniciado");
            Vector2 playerStartPosition = new Vector2(GameConfig.ScreenWidth / 2, GameConfig.ScreenHeight / 2);
            _playerFactory.CreatePlayer(_world, playerStartPosition);
            Console.WriteLine($"[Game1] Jogador criado em {playerStartPosition}");
        }

        private void RestartGame()
        {
            // Limpar todas as entidades
            var allEntities = _world.GetAllEntities().ToList();
            foreach (var entity in allEntities)
            {
                _world.RemoveEntity(entity);
            }

            // Recriar o jogador
            InitializeGame();
        }

        

        protected override void Update(GameTime gameTime)
        {
            bool upgradePending = _world.GetEntitiesWithComponent<Components.UpgradeRequestComponent>().Any();
            if (!upgradePending)
            {
                // Atualizar todos os sistemas
                _world.Update(gameTime);
            }
            
            if (_gameStateSystem.IsGameOver)
            {
                Console.WriteLine("[Game1] GameOver detectado, reiniciando...");
                RestartGame();
                _gameStateSystem.Reset();
            }

            var player = _world.GetEntitiesWithComponent<Components.PlayerInputComponent>().FirstOrDefault();
            if (player != null)
            {
                _cameraService.Update(player);
                _inputSystem?.SetCameraTransform(_cameraService.Transform);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Se não tivermos a textura do piso, mantenha o comportamento antigo
            if (_floorTexture == null)
            {
                GraphicsDevice.Clear(Color.ForestGreen);
            }
            else
            {
                // Limpar para preto antes de desenhar o piso em tiles
                GraphicsDevice.Clear(Color.Black);

                _spriteBatch.Begin(transformMatrix: _cameraService.Transform);

                const int tileSize = 128; // a imagem é 64x64
                int tilesX = (GameConfig.MapWidth + tileSize - 1) / tileSize;
                int tilesY = (GameConfig.MapHeight + tileSize - 1) / tileSize;

                for (int y = 0; y < tilesY; y++)
                {
                    for (int x = 0; x < tilesX; x++)
                    {
                        var dest = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                        _spriteBatch.Draw(_floorTexture, dest, Color.White);
                    }
                }

                _spriteBatch.End();
            }

            // Renderizar entidades usando a transformação da câmera
            _renderSystem.Draw(_cameraService.Transform);

            // Renderizar UI
            _uiSystem.Draw();
            
            // Renderizar inventário (hotbar sempre visível, full inventory quando aberto)
            _inventoryUISystem?.Draw();

            base.Draw(gameTime);
        }
    }
}
