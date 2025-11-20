using CubeSurvivor.World.Biomes;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;

namespace CubeSurvivor.Components.World
{
    // Marca uma entidade como pertencente a um bioma específico
    public sealed class BiomeComponent : Component
    {
        public BiomeType BiomeType { get; }

        public BiomeComponent(BiomeType biomeType)
        {
            BiomeType = biomeType;
        }
    }
}
