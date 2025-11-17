using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using CubeSurvivor.Systems;
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
            _graphics = new GraphicsDeviceManager(this);
            _world = new GameWorld();
            
            // Inicializar factories
            _playerFactory = new PlayerFactory();
            _enemyFactory = new EnemyFactory();
            _bulletFactory = new BulletFactory();
            
            // Inicializar serviço de câmera
            _cameraService = new CameraService(
                GameConfig.ScreenWidth, 
                GameConfig.ScreenHeight, 
                GameConfig.MapWidth, 
                GameConfig.MapHeight
            );
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Configurar tamanho da janela
            _graphics.PreferredBackBufferWidth = GameConfig.ScreenWidth;
            _graphics.PreferredBackBufferHeight = GameConfig.ScreenHeight;
        }

        protected override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Criar textura de pixel
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Tentar carregar textura do piso a partir da pasta assets (vários caminhos tentados)
            try
            {
                string[] candidates = new[]
                {
                    Path.Combine("assets", "floor.png"),
                    Path.Combine(Content.RootDirectory ?? "Content", "..", "assets", "floor.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "floor.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\assets\\floor.png")
                };

                var floorPath = candidates.FirstOrDefault(File.Exists);
                if (floorPath != null)
                {
                    using (var stream = File.OpenRead(floorPath))
                    {
                        _floorTexture = Texture2D.FromStream(GraphicsDevice, stream);
                    }
                }
                else
                {
                    _floorTexture = null;
                }
            }
            catch
            {
                _floorTexture = null;
            }

            // Tentar carregar font (se não existir, UI será sem texto)
            try
            {
                _font = Content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                _font = null; // UI funcionará sem texto
            }

            // Configurar sistemas
            _renderSystem = new RenderSystem(_spriteBatch);
            _renderSystem.Initialize(_world);
            _renderSystem.CreatePixelTexture(GraphicsDevice);

            _uiSystem = new UISystem(_spriteBatch, _font, _pixelTexture);
            _uiSystem.Initialize(_world);

            _gameStateSystem = new GameStateSystem();
            _gameStateSystem.Initialize(_world);
            // não chama RestartGame diretamente aqui — vamos checar IsGameOver depois do Update
            _gameStateSystem.OnGameOver += () => { /* flag disparada — Game1.Update irá reiniciar */ };

            _inputSystem = new PlayerInputSystem(_bulletFactory);
            _inputSystem.SetScreenSize(GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            _world.AddSystem(_inputSystem);
            _world.AddSystem(new AISystem());
            _world.AddSystem(new MovementSystem());
            _world.AddSystem(new BulletSystem());
            _world.AddSystem(new CollisionSystem());
            _world.AddSystem(new DeathSystem());
            _world.AddSystem(_gameStateSystem);

            // Sistema de spawn (spawna nas bordas do mapa finito)
            Rectangle spawnArea = new Rectangle(0, 0, GameConfig.MapWidth, GameConfig.MapHeight);
            _world.AddSystem(new EnemySpawnSystem(spawnArea, _enemyFactory, GameConfig.EnemySpawnInterval, GameConfig.MaxEnemies));

            // Criar jogador no centro da tela
            InitializeGame();
        }

        private void InitializeGame()
        {
            Vector2 playerStartPosition = new Vector2(GameConfig.ScreenWidth / 2, GameConfig.ScreenHeight / 2);
            _playerFactory.CreatePlayer(_world, playerStartPosition);
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
            // Atualizar todos os sistemas
            _world.Update(gameTime);
            
            // Se houve GameOver, executamos o restart uma vez aqui (fora do loop de sistemas)
            if (_gameStateSystem.IsGameOver)
            {
                RestartGame();
                _gameStateSystem.Reset();
            }

            // Atualizar câmera com base no jogador
            var player = _world.GetEntitiesWithComponent<Components.PlayerInputComponent>().FirstOrDefault();
            if (player != null)
            {
                _cameraService.Update(player);
                
                // Atualizar InputSystem com transformação da câmera
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

            base.Draw(gameTime);
        }
    }
}
