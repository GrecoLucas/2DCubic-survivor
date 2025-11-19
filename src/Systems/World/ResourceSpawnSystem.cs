using System;
using System.Linq;
using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema responsável por spawnar recursos (madeira) periodicamente no mapa.
    /// </summary>
    public sealed class ResourceSpawnSystem : GameSystem
    {
        private readonly WoodEntityFactory _woodFactory;
        private readonly LevelDefinition _levelDefinition;
        private readonly Random _random = new Random();

        private float _timer;

        public ResourceSpawnSystem(LevelDefinition levelDefinition, TextureManager textureManager = null)
        {
            _levelDefinition = levelDefinition ?? throw new ArgumentNullException(nameof(levelDefinition));
            _woodFactory = new WoodEntityFactory();
            if (textureManager != null)
            {
                _woodFactory.SetTextureManager(textureManager);
            }
        }

        public override void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer < GameConfig.WoodSpawnIntervalSeconds)
            {
                return;
            }

            _timer = 0f;

            // Verificar se há regiões de spawn configuradas no JSON
            if (_levelDefinition.WoodSpawnRegions.Count == 0)
            {
                return;
            }

            foreach (var region in _levelDefinition.WoodSpawnRegions)
            {
                SpawnWoodInRegion(region);
            }
        }

        private void SpawnWoodInRegion(WoodSpawnRegionDefinition region)
        {
            // Contar madeira ativa nesta região
            var pickups = World.GetEntitiesWithComponent<PickupComponent>()
                .Where(e =>
                {
                    var p = e.GetComponent<PickupComponent>();
                    var t = e.GetComponent<TransformComponent>();

                    if (p == null || t == null || p.Item == null)
                        return false;

                    return p.Item.Id == "wood" && region.Area.Contains(t.Position);
                })
                .ToList();

            if (pickups.Count >= region.MaxActiveWood)
            {
                return;
            }

            // Tentar spawnar nova madeira
            for (int attempt = 0; attempt < 10; attempt++)
            {
                float x = _random.Next(region.Area.Left, region.Area.Right);
                float y = _random.Next(region.Area.Top, region.Area.Bottom);
                var pos = new Vector2(x, y);

                // Verificar se a posição está livre
                if (IsPositionFree(pos))
                {
                    _woodFactory.CreateWood(World, pos, 1);
                    System.Console.WriteLine($"[ResourceSpawn] ✓ Madeira spawn em ({x:F0}, {y:F0})");
                    break;
                }
            }
        }

        private bool IsPositionFree(Vector2 position)
        {
            // Verificar se não há colliders no local
            foreach (var e in World.GetEntitiesWithComponent<ColliderComponent>())
            {
                var c = e.GetComponent<ColliderComponent>();
                var t = e.GetComponent<TransformComponent>();
                if (c == null || t == null)
                    continue;

                var bounds = c.GetBounds(t.Position);
                if (bounds.Contains(position))
                    return false;
            }

            return true;
        }
    }
}

