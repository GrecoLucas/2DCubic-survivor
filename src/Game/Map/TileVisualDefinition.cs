using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Defines visual variants and rotation behavior for a tile/block type.
    /// </summary>
    public sealed class TileVisualDefinition
    {
        /// <summary>
        /// Base ID of the tile/block type (e.g., "tree", "stone").
        /// </summary>
        public string BaseId { get; set; }

        /// <summary>
        /// Array of texture variants for this type.
        /// </summary>
        public Texture2D[] Variants { get; set; }

        /// <summary>
        /// Whether this type should have random rotation (0°, 90°, 180°, 270°).
        /// </summary>
        public bool AllowRandomRotation { get; set; }

        public TileVisualDefinition(string baseId, Texture2D[] variants, bool allowRandomRotation = false)
        {
            BaseId = baseId;
            Variants = variants ?? new Texture2D[0];
            AllowRandomRotation = allowRandomRotation;
        }
    }
}

