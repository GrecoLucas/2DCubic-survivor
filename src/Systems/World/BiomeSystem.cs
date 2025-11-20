using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CubeSurvivor.Core;
using CubeSurvivor.World.Biomes;
using System.Linq;

namespace CubeSurvivor.Systems.World
{
    // This system uses the Biome model defined in CubeSurvivor.World.Biomes namespace.
    /// <summary>
    /// Sistema responsável por gerenciar biomas do mundo e expor consultas reutilizáveis.
    /// </summary>
    public sealed class BiomeSystem : GameSystem
    {
        private readonly List<Biome> _biomes = new List<Biome>();

        public BiomeSystem(IEnumerable<Biome> initialBiomes = null)
        {
            if (initialBiomes != null)
                _biomes.AddRange(initialBiomes);
        }

        public void RegisterBiome(Biome b)
        {
            if (b == null) return;
            _biomes.Add(b);
        }

        public Biome GetBiomeAt(Vector2 pos)
        {
            return _biomes.FirstOrDefault(b => b.Contains(pos));
        }

        public Texture2D GetTextureForPosition(Vector2 pos)
        {
            return GetBiomeAt(pos)?.Texture;
        }

        public bool AllowsEnemySpawnsAt(Vector2 pos)
        {
            return GetBiomeAt(pos)?.AllowsEnemySpawns ?? false;
        }

        public override void Update(GameTime gameTime)
        {
            // service-style system; nothing per-frame for now
        }
    }
}
