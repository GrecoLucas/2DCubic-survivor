using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Component that identifies an entity as a map block.
    /// Stores tile coordinates and layer index so the block can be removed from the map when destroyed.
    /// </summary>
    public sealed class MapBlockComponent : Component
    {
        /// <summary>
        /// Tile X coordinate.
        /// </summary>
        public int TileX { get; set; }

        /// <summary>
        /// Tile Y coordinate.
        /// </summary>
        public int TileY { get; set; }

        /// <summary>
        /// Block layer index.
        /// </summary>
        public int LayerIndex { get; set; }

        public MapBlockComponent(int tileX, int tileY, int layerIndex)
        {
            TileX = tileX;
            TileY = tileY;
            LayerIndex = layerIndex;
        }
    }
}

