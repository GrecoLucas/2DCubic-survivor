using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;
using System;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema responsável por spawnar maçãs aleatoriamente no mapa.
    /// Spawna uma maçã a cada 10 segundos em uma posição aleatória.
    /// </summary>
    public sealed class AppleSpawnSystem : GameSystem
    {
        private readonly Rectangle _spawnArea;
        private readonly AppleEntityFactory _appleFactory;
        private readonly Random _random;
        
        private const float SpawnInterval = 10f; // 10 segundos
        private const int MaxApples = 20; // Limite máximo de maçãs no mapa
        
        private float _timeSinceLastSpawn;
        
        public AppleSpawnSystem(Rectangle spawnArea)
        {
            _spawnArea = spawnArea;
            _appleFactory = new AppleEntityFactory();
            _random = new Random();
            _timeSinceLastSpawn = 0f;
        }
        
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeSinceLastSpawn += deltaTime;
            
            // Verificar se é hora de spawnar
            if (_timeSinceLastSpawn >= SpawnInterval)
            {
                _timeSinceLastSpawn = 0f;
                
                // Contar maçãs existentes
                int currentAppleCount = 0;
                foreach (var entity in World.GetEntitiesWithComponent<Components.PickupComponent>())
                {
                    var pickup = entity.GetComponent<Components.PickupComponent>();
                    if (pickup != null && pickup.Item.Id == "apple")
                    {
                        currentAppleCount++;
                    }
                }
                
                // Só spawnar se não atingiu o limite
                if (currentAppleCount < MaxApples)
                {
                    SpawnApple();
                }
            }
        }
        
        private void SpawnApple()
        {
            // Gerar posição aleatória dentro da área de spawn
            float x = _spawnArea.X + (float)_random.NextDouble() * _spawnArea.Width;
            float y = _spawnArea.Y + (float)_random.NextDouble() * _spawnArea.Height;
            Vector2 spawnPosition = new Vector2(x, y);
            
            // Criar maçã na posição
            _appleFactory.CreateApple(World, spawnPosition);
        }
    }
}

