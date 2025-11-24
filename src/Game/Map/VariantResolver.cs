using System;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Deterministic variant resolver using hash-based selection.
    /// Same coordinates + seed = same variant and rotation, always.
    /// </summary>
    public sealed class VariantResolver : IVariantResolver
    {
        private readonly TileVisualCatalog _catalog;

        public VariantResolver(TileVisualCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public (Texture2D texture, float rotation) Resolve(string baseId, int tileX, int tileY, int layerIndex, int worldSeed)
        {
            var definition = _catalog.GetDefinition(baseId);
            if (definition == null || definition.Variants == null || definition.Variants.Length == 0)
            {
                return (null, 0f);
            }

            // Deterministic hash based on position, layer, and seed
            int hash = ComputeHash(tileX, tileY, layerIndex, worldSeed);

            // Select variant index
            int variantIndex = Math.Abs(hash) % definition.Variants.Length;
            Texture2D texture = definition.Variants[variantIndex];

            // Select rotation if allowed
            float rotation = 0f;
            if (definition.AllowRandomRotation)
            {
                // Use a different part of the hash for rotation
                int rotHash = Math.Abs(hash / 7); // Shift to get different distribution
                int rotIndex = rotHash % 4; // 0, 1, 2, 3 -> 0°, 90°, 180°, 270°
                rotation = rotIndex * MathF.PI / 2f; // Convert to radians
            }

            return (texture, rotation);
        }

        /// <summary>
        /// Computes a deterministic hash from tile coordinates, layer, and world seed.
        /// Uses prime number multipliers for good distribution.
        /// </summary>
        private static int ComputeHash(int x, int y, int layer, int seed)
        {
            // Prime number multipliers for good hash distribution
            return (x * 73856093) ^ (y * 19349663) ^ (layer * 83492791) ^ seed;
        }
    }
}

