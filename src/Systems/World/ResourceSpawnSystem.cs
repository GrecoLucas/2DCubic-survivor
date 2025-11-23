using System;
using System.Collections.Generic;
using System.Linq;
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using CubeSurvivor.Game.Map;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems.World
{
    /// <summary>
    /// V2 of Resource Spawn System using region-based spawning.
    /// Spawns wood, gold, and other resources in designated regions.
    /// </summary>
    public sealed class ResourceSpawnSystem : GameSystem
    {
        private readonly Systems.Core.ISpawnRegionProvider _regionProvider;
        private readonly TextureManager _textureManager;
        private readonly Random _random = new Random();

        // Track spawn timers per region type
        private readonly Dictionary<string, float> _regionTimers = new Dictionary<string, float>();

        public ResourceSpawnSystem(
            Systems.Core.ISpawnRegionProvider regionProvider,
            TextureManager textureManager)
        {
            _regionProvider = regionProvider ?? throw new ArgumentNullException(nameof(regionProvider));
            _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Process wood spawn regions
            ProcessSpawnRegions(RegionType.WoodSpawn, deltaTime, SpawnWood);

            // Process gold spawn regions
            ProcessSpawnRegions(RegionType.GoldSpawn, deltaTime, SpawnGold);

            // Process tree spawn regions (if needed)
            // Trees are typically handled by BlockLayer, but we could spawn pickup items here
        }

        private void ProcessSpawnRegions(
            RegionType regionType,
            float deltaTime,
            Action<RegionDefinition> spawnAction)
        {
            var regions = _regionProvider.GetRegions(regionType).ToList();

            foreach (var region in regions)
            {
                string timerKey = region.Id ?? $"{regionType}_{regions.IndexOf(region)}";

                // Initialize timer if needed
                if (!_regionTimers.ContainsKey(timerKey))
                {
                    _regionTimers[timerKey] = 0f;
                }

                _regionTimers[timerKey] += deltaTime;

                // Get spawn interval from region metadata
                float interval = 5f; // Default
                if (region.Meta.TryGetValue("intervalSeconds", out string intervalStr))
                {
                    if (float.TryParse(intervalStr, out float parsed))
                    {
                        interval = parsed;
                    }
                }

                // Check if it's time to spawn
                if (_regionTimers[timerKey] >= interval)
                {
                    _regionTimers[timerKey] = 0f;

                    // Get max active count
                    int maxActive = 10; // Default
                    if (region.Meta.TryGetValue("maxActive", out string maxStr))
                    {
                        if (int.TryParse(maxStr, out int parsed))
                        {
                            maxActive = parsed;
                }
                    }

                    // Count existing resources in this region
                    int currentCount = CountResourcesInRegion(region, regionType);

                    if (currentCount < maxActive)
                    {
                        spawnAction(region);
                    }
                }
            }
        }

        private void SpawnWood(RegionDefinition region)
        {
            // Get random tile within region (tile coordinates)
            int tileSize = _regionProvider.GetTileSize();
            Point randomTile = RegionHelpers.GetRandomTileInRegion(region.Area, _random);
            
            // Convert to world pixel position (center of tile)
            Vector2 position = RegionHelpers.TileToWorldCenter(randomTile, tileSize);

            var wood = World.CreateEntity("Wood");
            wood.AddComponent(new TransformComponent(position));
            
            var woodTexture = _textureManager.GetTexture("wood");
            if (woodTexture != null)
            {
                wood.AddComponent(new SpriteComponent(woodTexture, 32, 32, null, RenderLayer.GroundItems));
            }
            else
            {
                wood.AddComponent(new SpriteComponent(new Color(139, 90, 43), 32, 32, RenderLayer.GroundItems));
            }

            wood.AddComponent(new ColliderComponent(32, 32, ColliderTag.Pickup));
            
            var woodItem = new Inventory.Items.Resources.WoodItem();
            wood.AddComponent(new PickupComponent(woodItem, quantity: _random.Next(1, 4)));
        }

        private void SpawnGold(RegionDefinition region)
        {
            // Get random tile within region (tile coordinates)
            int tileSize = _regionProvider.GetTileSize();
            Point randomTile = RegionHelpers.GetRandomTileInRegion(region.Area, _random);
            
            // Convert to world pixel position (center of tile)
            Vector2 position = RegionHelpers.TileToWorldCenter(randomTile, tileSize);

            var gold = World.CreateEntity("Gold");
            gold.AddComponent(new TransformComponent(position));
            
            // Gold as a golden yellow pickup - TUDO 32x32
            gold.AddComponent(new SpriteComponent(new Color(255, 215, 0), 32, 32, RenderLayer.GroundItems));
            gold.AddComponent(new ColliderComponent(32, 32, ColliderTag.Pickup));
            
            var goldItem = new Inventory.Items.Resources.GoldItem();
            gold.AddComponent(new PickupComponent(goldItem, quantity: _random.Next(1, 3)));
        }

        private int CountResourcesInRegion(RegionDefinition region, RegionType regionType)
        {
            // Region area is already in tile coordinates
            Rectangle regionArea = region.Area;
            
            int count = 0;
            
            // Determine what item ID to look for based on region type
            string itemId = regionType switch
            {
                RegionType.WoodSpawn => "wood",
                RegionType.GoldSpawn => "gold",
                _ => null
            };

            if (itemId == null)
                return 0;

            foreach (var entity in World.GetEntitiesWithComponent<PickupComponent>())
            {
                var transform = entity.GetComponent<TransformComponent>();
                var pickup = entity.GetComponent<PickupComponent>();

                if (transform != null && pickup != null)
                {
                    // Convert entity world position to tile coordinates
                    int tileSize = _regionProvider.GetTileSize();
                    Point entityTile = new Point(
                        (int)(transform.Position.X / tileSize),
                        (int)(transform.Position.Y / tileSize)
                    );
                    
                    // Check if entity tile is within region bounds (regionArea is in tile coordinates)
                    if (regionArea.Contains(entityTile))
                    {
                        if (pickup.Item != null && pickup.Item.Id == itemId)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        // Helper method removed - using RegionHelpers.GetRandomTileInRegion instead
    }
}

