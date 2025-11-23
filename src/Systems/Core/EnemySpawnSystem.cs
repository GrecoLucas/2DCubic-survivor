using System;
using System.Linq;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using CubeSurvivor.Components;
using CubeSurvivor.Game.Map;

namespace CubeSurvivor.Systems.Core
{
    /// <summary>
    /// V2 of Enemy Spawn System using region-based spawning.
    /// Spawns enemies in designated EnemySpawn regions with configurable parameters.
    /// </summary>
    public sealed class EnemySpawnSystem : GameSystem
    {
        private readonly IEnemyFactory _enemyFactory;
        private readonly ISpawnRegionProvider _regionProvider;
        private readonly IEnemySpawnExclusionProvider _exclusionProvider;
        private readonly CubeSurvivor.Systems.World.BiomeSystem _biomeSystem;
        private readonly Random _random = new Random();
        
        private float _spawnTimer;
        private readonly float _baseSpawnInterval;
        private float _elapsedTime;
        private float _lastLoggedMultiplier = 0.01f;

        public EnemySpawnSystem(
            ISpawnRegionProvider regionProvider,
            IEnemyFactory enemyFactory,
            float baseSpawnInterval = 2f,
            IEnemySpawnExclusionProvider exclusionProvider = null,
            CubeSurvivor.Systems.World.BiomeSystem biomeSystem = null)
        {
            _regionProvider = regionProvider ?? throw new ArgumentNullException(nameof(regionProvider));
            _enemyFactory = enemyFactory ?? throw new ArgumentNullException(nameof(enemyFactory));
            _baseSpawnInterval = baseSpawnInterval;
            _exclusionProvider = exclusionProvider;
            _biomeSystem = biomeSystem;
            _spawnTimer = 0f;
            _elapsedTime = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _spawnTimer += deltaTime;
            _elapsedTime += deltaTime;

            // Count active enemies
            int enemyCount = World.GetEntitiesWithComponent<EnemyComponent>().Count();

            // Dynamic spawn interval
            float currentInterval = GameConfig.GetSpawnInterval(_baseSpawnInterval, _elapsedTime);

            // Check if player is in cave for accelerated spawning
            bool playerInCave = IsPlayerInCave();
            if (playerInCave)
            {
                currentInterval *= 0.08f; // Much faster in caves
            }

            // Get all enemy spawn regions
            var spawnRegions = _regionProvider.GetRegions(RegionType.EnemySpawn).ToList();
            if (spawnRegions.Count == 0)
            {
                return; // No spawn regions defined
            }

            // Calculate max enemies from regions
            int maxEnemies = CalculateMaxEnemies(spawnRegions, playerInCave);

            // Spawn if needed
            if (_spawnTimer >= currentInterval && enemyCount < maxEnemies)
            {
                _spawnTimer = 0f;
                SpawnEnemy(spawnRegions);
            }
        }

        private void SpawnEnemy(System.Collections.Generic.List<RegionDefinition> spawnRegions)
        {
            // Pick a random spawn region
            var region = spawnRegions[_random.Next(spawnRegions.Count)];

            // Try to find valid position
            const int maxAttempts = 10;
            Vector2? spawnPosition = null;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 pos = GetRandomPositionInRegion(region.Area);

                // Check biome restrictions
                if (_biomeSystem != null && !_biomeSystem.AllowsEnemySpawnsAt(pos))
                    continue;

                // Check exclusion zones
                if (_exclusionProvider != null)
                {
                    bool inExclusion = _exclusionProvider
                        .GetExclusionZones()
                        .Any(rect => rect.Contains(pos));
                    
                    if (inExclusion)
                        continue;
                }

                spawnPosition = pos;
                break;
            }

            if (!spawnPosition.HasValue)
                return; // Couldn't find valid position

            // Create enemy
            var enemy = _enemyFactory.CreateEnemy(World, spawnPosition.Value);

            // Apply difficulty scaling
            float multiplier = GameConfig.GetEnemyDifficultyMultiplier(_elapsedTime);

            var health = enemy.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.MaxHealth *= multiplier;
                health.CurrentHealth = health.MaxHealth;
            }

            var enemyComp = enemy.GetComponent<EnemyComponent>();
            if (enemyComp != null)
            {
                enemyComp.Damage *= multiplier;
            }

            // Log difficulty increases
            const float logThreshold = 0.05f;
            if (multiplier >= _lastLoggedMultiplier + logThreshold)
            {
                var ts = System.TimeSpan.FromSeconds(_elapsedTime);
                Console.WriteLine($"[EnemySpawnV2] {ts:mm\\:ss} - Difficulty: {multiplier:F2}x");
                _lastLoggedMultiplier = multiplier;
            }
        }

        private bool IsPlayerInCave()
        {
            if (_biomeSystem == null)
                return false;

            var player = World.GetEntitiesWithComponent<PlayerInputComponent>().FirstOrDefault();
            if (player == null)
                return false;

            var transform = player.GetComponent<TransformComponent>();
            if (transform == null)
                return false;

            var biome = _biomeSystem.GetBiomeAt(transform.Position);
            return biome != null && biome.Type == CubeSurvivor.World.Biomes.BiomeType.Cave;
        }

        private int CalculateMaxEnemies(System.Collections.Generic.List<RegionDefinition> regions, bool playerInCave)
        {
            int total = 50; // Default

            // Sum max enemies from all regions that define it
            foreach (var region in regions)
            {
                if (region.Meta.TryGetValue("maxEnemies", out string maxStr))
                {
                    if (int.TryParse(maxStr, out int max))
                    {
                        total = Math.Max(total, max);
                    }
                }
            }

            // Boost in caves
            if (playerInCave)
            {
                total = (int)(total * 1.8f);
            }

            return total;
        }

        private Vector2 GetRandomPositionInRegion(Rectangle area)
        {
            float x = area.X + (float)_random.NextDouble() * area.Width;
            float y = area.Y + (float)_random.NextDouble() * area.Height;
            return new Vector2(x, y);
        }
    }
}

