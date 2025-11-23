using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor
{
    /// <summary>
    /// Central editor state - single source of truth for active tool, brush, layer, etc.
    /// </summary>
    public class EditorContext
    {
        // Active tool
        public ToolType ActiveTool { get; set; } = ToolType.Brush;

        // Edit mode (tiles vs blocks)
        public EditMode EditMode { get; set; } = EditMode.Tiles;

        // Active layer
        public int ActiveLayerIndex { get; set; } = 0;

        // Active brush (tile ID or block type)
        public int ActiveBrushId { get; set; } = 1; // Default: Grass

        // Region creation
        public RegionType PendingRegionType { get; set; } = RegionType.PlayerSpawn;
        public Dictionary<string, string> PendingRegionMeta { get; set; } = new Dictionary<string, string>();

        // Selected region
        public string SelectedRegionId { get; set; }

        // Dirty flag
        public bool IsDirty { get; set; }

        // Map reference
        public ChunkedTileMap Map { get; set; }
        public MapDefinition MapDefinition { get; set; }

        // Camera reference (set by EditorState)
        public EditorCameraController Camera { get; set; }

        // ============================================================
        // COORDINATE CONVERSION HELPERS
        // ============================================================

        /// <summary>
        /// Converts screen point to world position using camera transform.
        /// </summary>
        public Vector2 ScreenToWorld(Point screen, Rectangle canvasBounds)
        {
            if (Camera == null) return screen.ToVector2();
            return Camera.ScreenToWorld(screen, canvasBounds);
        }

        /// <summary>
        /// Converts world position to tile coordinates.
        /// </summary>
        public Point WorldToTile(Vector2 world)
        {
            if (MapDefinition == null) return Point.Zero;
            int tileSize = MapDefinition.TileSize;
            int tx = (int)Math.Floor(world.X / tileSize);
            int ty = (int)Math.Floor(world.Y / tileSize);
            return new Point(tx, ty);
        }

        /// <summary>
        /// Converts screen point directly to tile coordinates.
        /// </summary>
        public Point ScreenToTile(Point screen, Rectangle canvasBounds)
        {
            Vector2 world = ScreenToWorld(screen, canvasBounds);
            return WorldToTile(world);
        }

        /// <summary>
        /// Checks if a tile coordinate is within map bounds.
        /// </summary>
        public bool IsValidTile(Point tile)
        {
            if (MapDefinition == null) return false;
            return tile.X >= 0 && tile.Y >= 0 && 
                   tile.X < MapDefinition.MapWidth && 
                   tile.Y < MapDefinition.MapHeight;
        }

        // ============================================================
        // REGION MANAGEMENT
        // ============================================================

        /// <summary>
        /// Deletes a region by ID.
        /// </summary>
        public void DeleteRegion(string id)
        {
            if (MapDefinition == null) return;
            int removed = MapDefinition.Regions.RemoveAll(r => r.Id == id);
            if (removed > 0)
            {
                IsDirty = true;
                EditorLogger.Log("Regions", $"Deleted region '{id}' (removed={removed})");
            }
            else
            {
                EditorLogger.LogWarning("Regions", $"Failed to delete region '{id}' (not found)");
            }
        }
    }

    public enum ToolType
    {
        Brush,
        Eraser,
        BoxFill,
        FloodFill,
        Picker,
        Region,
        SelectMove
    }

    public enum EditMode
    {
        Tiles,
        Blocks
    }
}

