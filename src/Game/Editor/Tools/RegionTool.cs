using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;

namespace CubeSurvivor.Game.Editor.Tools
{
    /// <summary>
    /// Region tool: click-drag to create spawn/safe regions.
    /// </summary>
    public class RegionTool : IEditorTool
    {
        private Vector2? _startWorld;
        private Vector2? _currentWorld;

        public void OnMouseDown(Point tilePos, MouseState mouseState, EditorContext context)
        {
            // Store tile coordinates (not world pixels)
            _startWorld = tilePos.ToVector2();
        }

        public void OnMouseDrag(Point tilePos, MouseState mouseState, EditorContext context)
        {
            // Store tile coordinates (not world pixels)
            _currentWorld = tilePos.ToVector2();
        }

        public void OnMouseUp(Point tilePos, MouseState mouseState, EditorContext context)
        {
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
                    // Create new region with tile coordinates
                    string id = $"{context.PendingRegionType}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                    RegionDefinition region = new RegionDefinition
                    {
                        Id = id,
                        Type = context.PendingRegionType,
                        Area = tileRect, // TILE COORDINATES, not pixels!
                        Meta = new System.Collections.Generic.Dictionary<string, string>(context.PendingRegionMeta)
                    };

                    // Use context method to ensure only one PlayerSpawn
                    context.AddRegion(region);
                }
            }

            _startWorld = null;
            _currentWorld = null;
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

