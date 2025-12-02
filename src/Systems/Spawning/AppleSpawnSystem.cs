using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema responsável por spawnar maçãs aleatoriamente no mapa.
    /// Spawna uma maçã a cada 10 segundos em uma posição aleatória.
    /// </summary>
    public sealed class AppleSpawnSystem : GameSystem
    {
        private List<AppleSpawnRegionDefinition> _spawnRegions = new List<AppleSpawnRegionDefinition>();
        private readonly AppleEntityFactory _appleFactory;
        private readonly Random _random;
        
        private const float SpawnInterval = 10f; // 10 segundos
        
        private float _timeSinceLastSpawn;
        
        public AppleSpawnSystem(TextureManager textureManager = null)
        {
            _appleFactory = new AppleEntityFactory();
            if (textureManager != null)
            {
                _appleFactory.SetTextureManager(textureManager);
            }
            _random = new Random();
            _timeSinceLastSpawn = 0f;
        }

        public void SetSpawnRegions(List<AppleSpawnRegionDefinition> regions)
        {
            _spawnRegions = regions ?? new List<AppleSpawnRegionDefinition>();
            _timeSinceLastSpawn = 0f; // Reset timer on region change
        }
        
        public override void Update(GameTime gameTime)
        {
            if (_spawnRegions.Count == 0)
                return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeSinceLastSpawn += deltaTime;
            
            // Verificar se é hora de spawnar
            if (_timeSinceLastSpawn >= SpawnInterval)
            {
                _timeSinceLastSpawn = 0f;
                TrySpawnApple();
            }
        }
        
        private void TrySpawnApple()
        {
            // Get all existing apples once
            var apples = new List<Vector2>();
            foreach (var entity in World.GetEntitiesWithComponent<Components.PickupComponent>())
            {
                var pickup = entity.GetComponent<Components.PickupComponent>();
                var transform = entity.GetComponent<Components.TransformComponent>();
                
                if (pickup != null && pickup.Item.Id == "apple" && transform != null)
                {
                    apples.Add(transform.Position);
                }
            }

            // Shuffle regions to avoid bias
            var regions = _spawnRegions.OrderBy(x => _random.Next()).ToList();

            foreach (var region in regions)
            {
                // Count apples in this region
                int countInRegion = 0;
                foreach (var applePos in apples)
                {
                    if (region.Area.Contains(applePos))
                    {
                        countInRegion++;
                    }
                }

                if (countInRegion < region.MaxActiveApples)
                {
                    SpawnAppleInRegion(region);
                    return; // Spawn one at a time
                }
            }
        }
        
        private void SpawnAppleInRegion(AppleSpawnRegionDefinition region)
        {
            // Gerar posição aleatória dentro da região
            float x = region.Area.X + (float)_random.NextDouble() * region.Area.Width;
            float y = region.Area.Y + (float)_random.NextDouble() * region.Area.Height;
            Vector2 spawnPosition = new Vector2(x, y);
            
            // Criar maçã na posição
            _appleFactory.CreateApple(World, spawnPosition);
        }
    }
}

