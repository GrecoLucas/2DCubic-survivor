using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;

namespace CubeSurvivor.Game.Editor
{
    /// <summary>
    /// Renders grid, hover highlight, ghost preview, and regions.
    /// NOW ALSO DRAWS TILES AND BLOCKS FROM ChunkedTileMap!
    /// </summary>
    public class EditorRenderer
    {
        private static bool _loggedFirstDraw = false;
        public void DrawGrid(SpriteBatch spriteBatch, Texture2D pixelTexture, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            if (context.MapDefinition == null) return;

            int tileSize = context.MapDefinition.TileSize;
            int mapWidth = context.MapDefinition.MapWidth * tileSize;
            int mapHeight = context.MapDefinition.MapHeight * tileSize;

            // Draw vertical lines
            for (int x = 0; x <= context.MapDefinition.MapWidth; x++)
            {
                Point screenTop = camera.WorldToScreen(new Vector2(x * tileSize, 0), canvasBounds);
                Point screenBottom = camera.WorldToScreen(new Vector2(x * tileSize, mapHeight), canvasBounds);

                if (screenTop.X >= canvasBounds.Left && screenTop.X <= canvasBounds.Right)
                {
                    DrawLine(spriteBatch, pixelTexture, screenTop, screenBottom, new Color(80, 80, 80, 150));
                }
            }

            // Draw horizontal lines
            for (int y = 0; y <= context.MapDefinition.MapHeight; y++)
            {
                Point screenLeft = camera.WorldToScreen(new Vector2(0, y * tileSize), canvasBounds);
                Point screenRight = camera.WorldToScreen(new Vector2(mapWidth, y * tileSize), canvasBounds);

                if (screenLeft.Y >= canvasBounds.Top && screenLeft.Y <= canvasBounds.Bottom)
                {
                    DrawLine(spriteBatch, pixelTexture, screenLeft, screenRight, new Color(80, 80, 80, 150));
                }
            }
        }

        public void DrawHoverHighlight(SpriteBatch spriteBatch, Texture2D pixelTexture, Point tilePos, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            if (context.MapDefinition == null) return;

            int tileSize = context.MapDefinition.TileSize;
            Vector2 worldPos = tilePos.ToVector2() * tileSize;

            Point screenTopLeft = camera.WorldToScreen(worldPos, canvasBounds);
            Point screenBottomRight = camera.WorldToScreen(worldPos + new Vector2(tileSize, tileSize), canvasBounds);

            Rectangle screenRect = new Rectangle(
                screenTopLeft.X,
                screenTopLeft.Y,
                screenBottomRight.X - screenTopLeft.X,
                screenBottomRight.Y - screenTopLeft.Y
            );

            DrawBorder(spriteBatch, pixelTexture, screenRect, Color.Yellow, 2);
        }

        public void DrawGhostPreview(SpriteBatch spriteBatch, Texture2D pixelTexture, Point tilePos, EditorContext context, EditorCameraController camera, Rectangle canvasBounds, Texture2D spriteTexture)
        {
            if (context.MapDefinition == null) return;
            if (context.ActiveTool != ToolType.Brush && context.ActiveTool != ToolType.Eraser) return;

            int tileSize = context.MapDefinition.TileSize;
            Vector2 worldPos = tilePos.ToVector2() * tileSize;

            Point screenTopLeft = camera.WorldToScreen(worldPos, canvasBounds);
            Point screenBottomRight = camera.WorldToScreen(worldPos + new Vector2(tileSize, tileSize), canvasBounds);

            Rectangle screenRect = new Rectangle(
                screenTopLeft.X,
                screenTopLeft.Y,
                screenBottomRight.X - screenTopLeft.X,
                screenBottomRight.Y - screenTopLeft.Y
            );

            Color ghostColor = new Color(255, 255, 255, 120);
            if (spriteTexture != null)
            {
                spriteBatch.Draw(spriteTexture, screenRect, ghostColor);
            }
            else
            {
                // Fallback color
                Color fallback = GetFallbackColor(context.ActiveBrushId, context.EditMode);
                spriteBatch.Draw(pixelTexture, screenRect, fallback * 0.5f);
            }
        }

        public void DrawRegions(SpriteBatch spriteBatch, Texture2D pixelTexture, EditorContext context, EditorCameraController camera, Rectangle canvasBounds, SpriteFont font)
        {
            if (context.MapDefinition == null) return;

            foreach (var region in context.MapDefinition.Regions)
            {
                Point screenTopLeft = camera.WorldToScreen(region.Area.Location.ToVector2(), canvasBounds);
                Point screenBottomRight = camera.WorldToScreen(new Vector2(region.Area.Right, region.Area.Bottom), canvasBounds);

                Rectangle screenRect = new Rectangle(
                    screenTopLeft.X,
                    screenTopLeft.Y,
                    screenBottomRight.X - screenTopLeft.X,
                    screenBottomRight.Y - screenTopLeft.Y
                );

                Color regionColor = GetRegionColor(region.Type);
                spriteBatch.Draw(pixelTexture, screenRect, regionColor * 0.3f);

                int borderWidth = region.Id == context.SelectedRegionId ? 3 : 1;
                DrawBorder(spriteBatch, pixelTexture, screenRect, regionColor, borderWidth);

                // Label
                if (font != null)
                {
                    string label = $"{region.Type}\n{region.Id}";
                    spriteBatch.DrawString(font, label, screenTopLeft.ToVector2() + new Vector2(4, 4), Color.White);
                }
            }
        }

        private Color GetRegionColor(RegionType type)
        {
            return type switch
            {
                RegionType.PlayerSpawn => Color.Cyan,
                RegionType.EnemySpawn => Color.Red,
                RegionType.WoodSpawn => Color.SaddleBrown,
                RegionType.GoldSpawn => Color.Gold,
                RegionType.SafeZone => Color.Green,
                _ => Color.Gray
            };
        }

        private Color GetFallbackColor(int brushId, EditMode mode)
        {
            if (mode == EditMode.Tiles)
            {
                return brushId switch
                {
                    1 => new Color(50, 200, 50), // Grass
                    _ => Color.Gray
                };
            }
            else
            {
                return (BlockType)brushId switch
                {
                    BlockType.Wall => Color.Gray,
                    BlockType.Crate => new Color(150, 100, 50),
                    BlockType.Tree => new Color(34, 139, 34),
                    BlockType.Rock => new Color(100, 100, 120),
                    _ => Color.Transparent
                };
            }
        }

        private void DrawLine(SpriteBatch sb, Texture2D px, Point start, Point end, Color color)
        {
            if (start.X == end.X)
            {
                // Vertical
                sb.Draw(px, new Rectangle(start.X, System.Math.Min(start.Y, end.Y), 1, System.Math.Abs(end.Y - start.Y)), color);
            }
            else
            {
                // Horizontal
                sb.Draw(px, new Rectangle(System.Math.Min(start.X, end.X), start.Y, System.Math.Abs(end.X - start.X), 1), color);
            }
        }

        /// <summary>
        /// Draws TILES from ChunkedTileMap - this is the KEY method that makes painting visible!
        /// </summary>
        public void DrawTiles(SpriteBatch spriteBatch, Texture2D pixelTexture, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            if (context.Map == null || context.MapDefinition == null) return;

            int tileSize = context.MapDefinition.TileSize;
            int mapWidth = context.MapDefinition.MapWidth;
            int mapHeight = context.MapDefinition.MapHeight;

            // Determine visible tile range (simple culling)
            Vector2 topLeft = camera.ScreenToWorld(canvasBounds.Location, canvasBounds);
            Vector2 bottomRight = camera.ScreenToWorld(new Point(canvasBounds.Right, canvasBounds.Bottom), canvasBounds);

            int startX = Math.Max(0, (int)Math.Floor(topLeft.X / tileSize));
            int startY = Math.Max(0, (int)Math.Floor(topLeft.Y / tileSize));
            int endX = Math.Min(mapWidth, (int)Math.Ceiling(bottomRight.X / tileSize) + 1);
            int endY = Math.Min(mapHeight, (int)Math.Ceiling(bottomRight.Y / tileSize) + 1);

            if (!_loggedFirstDraw)
            {
                EditorLogger.Log("Renderer", $"DrawTiles: visible range ({startX},{startY}) to ({endX},{endY})");
                _loggedFirstDraw = true;
            }

            int tilesDrawn = 0;

            // Draw all visible tiles
            for (int layerIndex = 0; layerIndex < context.MapDefinition.TileLayers.Count; layerIndex++)
            {
                for (int ty = startY; ty < endY; ty++)
                {
                    for (int tx = startX; tx < endX; tx++)
                    {
                        int tileId = context.Map.GetTileAt(tx, ty, layerIndex);
                        if (tileId == 0) continue; // Empty

                        Vector2 worldPos = new Vector2(tx * tileSize, ty * tileSize);
                        Point screenTopLeft = camera.WorldToScreen(worldPos, canvasBounds);
                        Point screenBottomRight = camera.WorldToScreen(worldPos + new Vector2(tileSize, tileSize), canvasBounds);

                        Rectangle screenRect = new Rectangle(
                            screenTopLeft.X,
                            screenTopLeft.Y,
                            screenBottomRight.X - screenTopLeft.X,
                            screenBottomRight.Y - screenTopLeft.Y
                        );

                        // Fallback color (very visible!)
                        Color fallbackColor = GetTileFallbackColor(tileId);
                        spriteBatch.Draw(pixelTexture, screenRect, fallbackColor);

                        tilesDrawn++;
                    }
                }
            }

            if (tilesDrawn > 0 && !_loggedFirstDraw)
            {
                EditorLogger.Log("Renderer", $"Drew {tilesDrawn} tiles");
            }
        }

        /// <summary>
        /// Draws BLOCKS from ChunkedTileMap
        /// </summary>
        public void DrawBlocks(SpriteBatch spriteBatch, Texture2D pixelTexture, EditorContext context, EditorCameraController camera, Rectangle canvasBounds)
        {
            if (context.Map == null || context.MapDefinition == null) return;

            int tileSize = context.MapDefinition.TileSize;
            int mapWidth = context.MapDefinition.MapWidth;
            int mapHeight = context.MapDefinition.MapHeight;

            // Determine visible tile range
            Vector2 topLeft = camera.ScreenToWorld(canvasBounds.Location, canvasBounds);
            Vector2 bottomRight = camera.ScreenToWorld(new Point(canvasBounds.Right, canvasBounds.Bottom), canvasBounds);

            int startX = Math.Max(0, (int)Math.Floor(topLeft.X / tileSize));
            int startY = Math.Max(0, (int)Math.Floor(topLeft.Y / tileSize));
            int endX = Math.Min(mapWidth, (int)Math.Ceiling(bottomRight.X / tileSize) + 1);
            int endY = Math.Min(mapHeight, (int)Math.Ceiling(bottomRight.Y / tileSize) + 1);

            int blocksDrawn = 0;

            // Draw all visible blocks
            for (int layerIndex = 0; layerIndex < context.MapDefinition.BlockLayers.Count; layerIndex++)
            {
                for (int ty = startY; ty < endY; ty++)
                {
                    for (int tx = startX; tx < endX; tx++)
                    {
                        BlockType blockType = context.Map.GetBlockAtTile(tx, ty, layerIndex);
                        if (blockType == BlockType.Empty) continue;

                        Vector2 worldPos = new Vector2(tx * tileSize, ty * tileSize);
                        Point screenTopLeft = camera.WorldToScreen(worldPos, canvasBounds);
                        Point screenBottomRight = camera.WorldToScreen(worldPos + new Vector2(tileSize, tileSize), canvasBounds);

                        Rectangle screenRect = new Rectangle(
                            screenTopLeft.X,
                            screenTopLeft.Y,
                            screenBottomRight.X - screenTopLeft.X,
                            screenBottomRight.Y - screenTopLeft.Y
                        );

                        // Fallback color (very visible!)
                        Color fallbackColor = GetBlockFallbackColor(blockType);
                        spriteBatch.Draw(pixelTexture, screenRect, fallbackColor);

                        blocksDrawn++;
                    }
                }
            }
        }

        private Color GetTileFallbackColor(int tileId)
        {
            return tileId switch
            {
                1 => new Color(50, 220, 50, 255),    // Grass - bright green
                2 => new Color(100, 100, 255, 255),  // Water - blue
                _ => new Color(200, 200, 200, 255)   // Unknown - light gray
            };
        }

        private Color GetBlockFallbackColor(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Wall => new Color(120, 120, 120, 255),   // Gray
                BlockType.Crate => new Color(180, 120, 60, 255),   // Brown
                BlockType.Tree => new Color(34, 180, 34, 255),     // Green
                BlockType.Rock => new Color(100, 100, 150, 255),   // Blue-gray
                _ => new Color(255, 0, 255, 255)                   // Magenta (error)
            };
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

