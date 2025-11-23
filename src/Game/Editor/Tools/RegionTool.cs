using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Region tool: click-drag to create spawn/safe regions.
    /// Also supports clicking to select regions and Delete key to remove them.
    /// </summary>
    public class RegionTool : IEditorTool
    {
        private Vector2? _startWorld;
        private Vector2? _currentWorld;
        private bool _isDragging = false;

        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            // Erase mode: delete region at click position
            if (context.RegionEraseMode)
            {
                TryErase(tilePos, context);
                return;
            }

            // Check if clicking on an existing region first (hit-test from last to first for proper selection)
            RegionDefinition clickedRegion = FindRegionAtTile(tilePos, context);
            if (clickedRegion != null)
            {
                // Select this region WITHOUT changing the placement type
                // ActiveRegionTypeToPlace remains unchanged - user can still place new regions of the chosen type
                context.SelectedRegionId = clickedRegion.Id;
                context.SelectedRegionRef = clickedRegion;
                EditorLogger.Log("RegionTool", $"Selected existing region: {clickedRegion.Id} ({clickedRegion.Type}). Placement type remains: {context.ActiveRegionTypeToPlace}");
                _isDragging = false;
                return; // IMPORTANT: don't start creating
            }

            // Otherwise start dragging to create new region
            _startWorld = tilePos.ToVector2();
            _isDragging = true;
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context)
        {
            // Store tile coordinates (not world pixels)
            _currentWorld = tilePos.ToVector2();
        }

        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context)
        {
            // Only create region if we were actually dragging
            if (!_isDragging) return;
            
            if (_startWorld.HasValue && context.MapDefinition != null)
            {
                // Use tile coordinates directly (snap to tile grid)
                Point startTile = new Point((int)_startWorld.Value.X, (int)_startWorld.Value.Y);
                Point endTile = tilePos;
                
                // Snap to tile grid and create rectangle in tile coordinates
                Rectangle tileRect = GetTileRect(startTile, endTile);
                
                // Clamp to map bounds
                tileRect.X = Math.Max(0, Math.Min(tileRect.X, context.MapDefinition.MapWidth - 1));
                tileRect.Y = Math.Max(0, Math.Min(tileRect.Y, context.MapDefinition.MapHeight - 1));
                tileRect.Width = Math.Min(tileRect.Width, context.MapDefinition.MapWidth - tileRect.X);
                tileRect.Height = Math.Min(tileRect.Height, context.MapDefinition.MapHeight - tileRect.Y);

                if (tileRect.Width > 0 && tileRect.Height > 0)
                {
                    // Use ActiveRegionTypeToPlace from Region Palette
                    RegionType regionType = context.ActiveRegionTypeToPlace;
                    int tileSize = context.MapDefinition.TileSize;
                    string mapPath = context.MapDefinition != null ? "unknown" : "null";
                    
                    // Get defaults for this region type
                    var defaults = RegionDefaults.GetDefaults(regionType);
                    
                    // Generate ID based on type
                    string typePrefix = regionType.ToString().ToLowerInvariant();
                    string id = $"{typePrefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                    
                    // EXTENSIVE DEBUG LOG: Region creation
                    EditorLogger.Log("RegionTool", "=== CreateRegion ===");
                    EditorLogger.Log("RegionTool", $"  Map={mapPath}");
                    EditorLogger.Log("RegionTool", $"  Type={regionType} ({(int)regionType})");
                    EditorLogger.Log("RegionTool", $"  StartTile=({startTile.X},{startTile.Y}) EndTile=({endTile.X},{endTile.Y})");
                    EditorLogger.Log("RegionTool", $"  AreaTiles: L={tileRect.Left} R={tileRect.Right} T={tileRect.Top} B={tileRect.Bottom}");
                    EditorLogger.Log("RegionTool", $"  SizeTiles: W={tileRect.Width} H={tileRect.Height}");
                    EditorLogger.Log("RegionTool", $"  TileSizePx={tileSize}");
                    EditorLogger.Log("RegionTool", $"  AreaWorldPx: X={tileRect.Left * tileSize} Y={tileRect.Top * tileSize} W={tileRect.Width * tileSize} H={tileRect.Height * tileSize}");
                    
                    RegionDefinition region = new RegionDefinition
                    {
                        Id = id,
                        Type = regionType,
                        Area = tileRect, // TILE COORDINATES, not pixels!
                        Meta = new System.Collections.Generic.Dictionary<string, string>(defaults)
                    };

                    // Use context method to ensure only one PlayerSpawn
                    context.AddRegion(region);
                    
                    // Select the newly created region
                    context.SelectedRegionId = region.Id;
                    context.SelectedRegionRef = region;
                    
                    EditorLogger.Log("RegionTool", $"Created region: {id} ({regionType}) - Total regions now: {context.MapDefinition.Regions.Count}");
                }
            }

            _startWorld = null;
            _currentWorld = null;
            _isDragging = false;
        }

        /// <summary>
        /// Finds a region that contains the given tile coordinate.
        /// Returns the LAST matching region (topmost in draw order) for proper selection.
        /// </summary>
        private RegionDefinition FindRegionAtTile(Point tilePos, EditorContext context)
        {
            if (context.MapDefinition == null) return null;

            // Search from last to first to get the topmost region
            for (int i = context.MapDefinition.Regions.Count - 1; i >= 0; i--)
            {
                var region = context.MapDefinition.Regions[i];
                // Region.Area is in tile coordinates
                if (region.Area.Contains(tilePos))
                {
                    return region;
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to erase a region at the given tile position.
        /// </summary>
        private void TryErase(Point tilePos, EditorContext context)
        {
            var hit = FindRegionAtTile(tilePos, context);
            if (hit == null) return;

            // Optional: protect PlayerSpawn (comment out if you want to allow erasing it)
            if (hit.Type == RegionType.PlayerSpawn)
            {
                EditorLogger.Log("RegionTool", "Cannot erase PlayerSpawn region");
                return;
            }

            context.DeleteRegion(hit.Id);
            context.SelectedRegionId = null;
            context.SelectedRegionRef = null;

            EditorLogger.Log("RegionTool", $"Erased region {hit.Id} ({hit.Type})");
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            if (_startWorld.HasValue && _currentWorld.HasValue && context.MapDefinition != null)
            {
                // Convert tile coordinates to world pixels for drawing
                int tileSize = context.MapDefinition.TileSize;
                Point startTile = new Point((int)_startWorld.Value.X, (int)_startWorld.Value.Y);
                Point endTile = new Point((int)_currentWorld.Value.X, (int)_currentWorld.Value.Y);
                Rectangle tileRect = GetTileRect(startTile, endTile);
                
                // Convert to world pixels
                Rectangle worldRect = new Rectangle(
                    tileRect.X * tileSize,
                    tileRect.Y * tileSize,
                    tileRect.Width * tileSize,
                    tileRect.Height * tileSize
                );

                Point screenTopLeft = camera.WorldToScreen(worldRect.Location.ToVector2(), canvasBounds);
                Point screenBottomRight = camera.WorldToScreen(new Vector2(worldRect.Right, worldRect.Bottom), canvasBounds);
                Rectangle screenRect = new Rectangle(
                    screenTopLeft.X,
                    screenTopLeft.Y,
                    screenBottomRight.X - screenTopLeft.X,
                    screenBottomRight.Y - screenTopLeft.Y
                );

                spriteBatch.Draw(pixelTexture, screenRect, new Color(255, 200, 100, 80));
                DrawBorder(spriteBatch, pixelTexture, screenRect, Color.Orange, 2);
            }
        }

        private Rectangle GetTileRect(Point a, Point b)
        {
            int minX = Math.Min(a.X, b.X);
            int minY = Math.Min(a.Y, b.Y);
            int maxX = Math.Max(a.X, b.X) + 1; // +1 because rectangle is exclusive on right/bottom
            int maxY = Math.Max(a.Y, b.Y) + 1;
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private void DrawBorder(SpriteBatch sb, Texture2D px, Rectangle rect, Color color, int width)
        {
            sb.Draw(px, new Rectangle(rect.X, rect.Y, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Bottom - width, rect.Width, width), color);
            sb.Draw(px, new Rectangle(rect.X, rect.Y, width, rect.Height), color);
            sb.Draw(px, new Rectangle(rect.Right - width, rect.Y, width, rect.Height), color);
        }
    }
}

