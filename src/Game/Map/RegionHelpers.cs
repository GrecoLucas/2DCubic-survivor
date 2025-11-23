using System;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Helper utilities for working with regions (tile-coordinate based).
    /// </summary>
    public static class RegionHelpers
    {
        /// <summary>
        /// Gets a random tile coordinate within a region (tile coordinates).
        /// </summary>
        /// <param name="regionArea">Region area in tile coordinates.</param>
        /// <param name="rng">Random number generator.</param>
        /// <returns>Random tile point within the region bounds.</returns>
        public static Point GetRandomTileInRegion(Rectangle regionArea, Random rng)
        {
            // RegionArea is in tile coordinates, Right/Bottom are exclusive
            int x = rng.Next(regionArea.Left, regionArea.Right);
            int y = rng.Next(regionArea.Top, regionArea.Bottom);
            return new Point(x, y);
        }

        /// <summary>
        /// Clamps a region area to map bounds (tile coordinates).
        /// </summary>
        public static Rectangle ClampRegionToMap(Rectangle regionArea, int mapWidth, int mapHeight)
        {
            int x = Math.Max(0, Math.Min(regionArea.X, mapWidth - 1));
            int y = Math.Max(0, Math.Min(regionArea.Y, mapHeight - 1));
            int right = Math.Min(regionArea.Right, mapWidth);
            int bottom = Math.Min(regionArea.Bottom, mapHeight);
            int width = Math.Max(0, right - x);
            int height = Math.Max(0, bottom - y);
            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Converts a tile coordinate to world pixel position (center of tile).
        /// </summary>
        public static Vector2 TileToWorldCenter(Point tile, int tileSize)
        {
            return new Vector2(
                tile.X * tileSize + tileSize / 2f,
                tile.Y * tileSize + tileSize / 2f
            );
        }
    }
}

