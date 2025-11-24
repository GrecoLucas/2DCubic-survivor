using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Resolves deterministic visual variants and rotations for tiles/blocks.
    /// Pure service interface - no graphics dependencies in implementation.
    /// </summary>
    public interface IVariantResolver
    {
        /// <summary>
        /// Resolves the texture variant and rotation for a tile/block at the given position.
        /// </summary>
        /// <param name="baseId">Base ID of the tile/block type (e.g., "tree", "stone")</param>
        /// <param name="tileX">Tile X coordinate</param>
        /// <param name="tileY">Tile Y coordinate</param>
        /// <param name="layerIndex">Layer index (0-based)</param>
        /// <param name="worldSeed">World seed for deterministic generation</param>
        /// <returns>Tuple of (texture, rotation in radians)</returns>
        (Texture2D texture, float rotation) Resolve(string baseId, int tileX, int tileY, int layerIndex, int worldSeed);
    }
}

