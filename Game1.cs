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
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Sistema ECS
        private GameWorld _world;
        private RenderSystem _renderSystem;
        private UISystem _uiSystem;
        private GameStateSystem _gameStateSystem;
        private InputSystem _inputSystem;

        // Configurações da tela
        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        // Limites do mapa (mapa finito). Ajuste conforme necessário.
        private const int MapWidth = 2000;
        private const int MapHeight = 2000;

        // Matriz de câmera usada no render
        private Matrix? _cameraTransform;

        // Font para UI (vamos criar uma simples)
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private Texture2D _floorTexture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Configurar tamanho da janela
            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
        }

        protected override void Initialize()
        {
            // Criar o mundo ECS
            _world = new GameWorld();

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

            _inputSystem = new InputSystem();
            _inputSystem.SetScreenSize(ScreenWidth, ScreenHeight);
            _world.AddSystem(_inputSystem);
            _world.AddSystem(new AISystem());
            _world.AddSystem(new MovementSystem());
            _world.AddSystem(new BulletSystem());
            _world.AddSystem(new CollisionSystem());
            _world.AddSystem(new DeathSystem());
            _world.AddSystem(_gameStateSystem);

            // Sistema de spawn (spawna nas bordas do mapa finito)
            Rectangle spawnArea = new Rectangle(0, 0, MapWidth, MapHeight);
            _world.AddSystem(new EnemySpawnSystem(spawnArea, 2f, 100)); // 2s intervalo, max 100 inimigos

            // Criar jogador no centro da tela
            InitializeGame();
        }

        private void InitializeGame()
        {
            Vector2 playerStartPosition = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            PlayerEntity.Create(_world, playerStartPosition);
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
                // reset camera para centro do jogador recém-criado
                var p = _world.GetEntitiesWithComponent<Components.InputComponent>().FirstOrDefault();
                if (p != null)
                {
                    var t = p.GetComponent<Components.TransformComponent>();
                    if (t != null)
                    {
                        _cameraTransform = Matrix.CreateTranslation(-t.Position.X + ScreenWidth / 2f, -t.Position.Y + ScreenHeight / 2f, 0f);
                    }
                }
            }

            // Clamp da posição do jogador dentro dos limites do mapa e atualizar câmera
            var player = _world.GetEntitiesWithComponent<Components.InputComponent>().FirstOrDefault();
            if (player != null)
            {
                var t = player.GetComponent<Components.TransformComponent>();
                var s = player.GetComponent<Components.SpriteComponent>();
                if (t != null && s != null)
                {
                    var half = s.Size / 2f;
                    t.Position = new Vector2(
                        MathHelper.Clamp(t.Position.X, half.X, MapWidth - half.X),
                        MathHelper.Clamp(t.Position.Y, half.Y, MapHeight - half.Y)
                    );

                    // Calcular posição da câmera centralizada no jogador, mas dentro dos limites do mapa
                    float camX = t.Position.X;
                    float camY = t.Position.Y;

                    camX = MathHelper.Clamp(camX, ScreenWidth / 2f, MapWidth - ScreenWidth / 2f);
                    camY = MathHelper.Clamp(camY, ScreenHeight / 2f, MapHeight - ScreenHeight / 2f);

                    _cameraTransform = Matrix.CreateTranslation(-camX + ScreenWidth / 2f, -camY + ScreenHeight / 2f, 0f);
                }
            }

            // Atualizar InputSystem com transformação da câmera
            if (_inputSystem != null)
            {
                _inputSystem.SetCameraTransform(_cameraTransform);
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

                var cam = _cameraTransform ?? Matrix.Identity;
                _spriteBatch.Begin(transformMatrix: cam);

                const int tileSize = 128; // a imagem é 64x64
                int tilesX = (MapWidth + tileSize - 1) / tileSize;
                int tilesY = (MapHeight + tileSize - 1) / tileSize;

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
            _renderSystem.Draw(_cameraTransform);

            // Renderizar UI
            _uiSystem.Draw();

            base.Draw(gameTime);
        }
    }
}
