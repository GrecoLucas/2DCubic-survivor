using System;
using System.Linq;
using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems.World
{
    /// <summary>
    /// Sistema responsável por spawnar árvores iniciais baseado no treeDensity dos biomas.
    /// Executado uma vez na inicialização do mundo.
    /// </summary>
    public sealed class BiomeTreeSpawnSystem : GameSystem
    {
        private readonly WoodEntityFactory _woodFactory;
        private readonly BiomeSystem _biomeSystem;
        private readonly Random _random = new Random();
        private bool _hasSpawned = false;

        public BiomeTreeSpawnSystem(BiomeSystem biomeSystem, TextureManager textureManager = null)
        {
            _biomeSystem = biomeSystem ?? throw new ArgumentNullException(nameof(biomeSystem));
            _woodFactory = new WoodEntityFactory();
            
            if (textureManager != null)
            {
                _woodFactory.SetTextureManager(textureManager);
            }
        }

        public override void Update(GameTime gameTime)
        {
            // Executar apenas uma vez
            if (_hasSpawned)
                return;

            _hasSpawned = true;
            SpawnInitialTrees();
        }

        private void SpawnInitialTrees()
        {
            // Para cada bioma com treeDensity > 0, spawnar árvores
            var biomes = GetBiomesFromSystem();
            
            foreach (var biome in biomes)
            {
                if (biome.TreeDensity <= 0)
                    continue;

                Console.WriteLine($"[BiomeTreeSpawn] Spawnando {biome.TreeDensity} árvores no bioma {biome.Type}...");
                
                int spawned = 0;
                int maxAttempts = biome.TreeDensity * 3; // Tentar até 3x mais para compensar colisões
                
                for (int i = 0; i < maxAttempts && spawned < biome.TreeDensity; i++)
                {
                    // Gerar posição aleatória dentro da área do bioma
                    float x = _random.Next(biome.Area.Left, biome.Area.Right);
                    float y = _random.Next(biome.Area.Top, biome.Area.Bottom);
                    var pos = new Vector2(x, y);

                    // Verificar se a posição está livre
                    if (!IsPositionFree(pos))
                        continue;

                    // Spawnar árvore
                    _woodFactory.CreateWood(World, pos, _random.Next(1, 4)); // 1-3 madeiras por árvore
                    spawned++;
                }

                Console.WriteLine($"[BiomeTreeSpawn] ✓ {spawned}/{biome.TreeDensity} árvores criadas no bioma {biome.Type}");
            }
        }

        private bool IsPositionFree(Vector2 position)
        {
            const float minDistance = 80f; // Distância mínima entre árvores
            
            // Verificar se não há outros colliders muito próximos
            foreach (var e in World.GetEntitiesWithComponent<ColliderComponent>())
            {
                var t = e.GetComponent<TransformComponent>();
                if (t == null)
                    continue;

                float distance = Vector2.Distance(position, t.Position);
                if (distance < minDistance)
                    return false;
            }

            return true;
        }

        private System.Collections.Generic.IEnumerable<CubeSurvivor.World.Biomes.Biome> GetBiomesFromSystem()
        {
            return _biomeSystem.GetAllBiomes();
        }
    }
}
