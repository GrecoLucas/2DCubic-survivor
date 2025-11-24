using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Game.Editor.Diagnostics;
using CubeSurvivor.Game.Editor.UI;

namespace CubeSurvivor.Game.Editor
{
    /// <summary>
    /// Renders grid, hover highlight, ghost preview, and regions.
    /// NOW ALSO DRAWS TILES AND BLOCKS FROM ChunkedTileMap!
    /// </summary>
    public class EditorRenderer
    {
        private static bool _loggedFirstDraw = false; // For DrawTiles
        private static bool _loggedFirstDrawRegions = false; // For DrawRegions
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

            int tileSize = context.MapDefinition.TileSize;
            Vector2 cameraPos = camera?.Position ?? Vector2.Zero;

            // EXTENSIVE DEBUG LOG: First draw per session
            if (!_loggedFirstDrawRegions && context.MapDefinition.Regions != null && context.MapDefinition.Regions.Count > 0)
            {
                EditorLogger.Log("EditorRenderer", "=== DrawRegions (first time) ===");
                EditorLogger.Log("EditorRenderer", $"  tileSize={tileSize} camera=({cameraPos.X:F1},{cameraPos.Y:F1})");
                int count = Math.Min(3, context.MapDefinition.Regions.Count);
                for (int i = 0; i < count; i++)
                {
                    var region = context.MapDefinition.Regions[i];
                    Rectangle worldRect = new Rectangle(
                        region.Area.X * tileSize,
                        region.Area.Y * tileSize,
                        region.Area.Width * tileSize,
                        region.Area.Height * tileSize
                    );
                    EditorLogger.Log("EditorRenderer", $"  region[{i}] {region.Id}: tiles L={region.Area.Left} R={region.Area.Right} T={region.Area.Top} B={region.Area.Bottom}");
                    EditorLogger.Log("EditorRenderer", $"    -> worldPxRect X={worldRect.X} Y={worldRect.Y} W={worldRect.Width} H={worldRect.Height}");
                }
                _loggedFirstDrawRegions = true;
            }

            foreach (var region in context.MapDefinition.Regions)
            {
                // Convert tile coordinates to world pixels
                // Region.Area is in tile coordinates (Left, Top, Width, Height)
                Rectangle worldRect = new Rectangle(
                    region.Area.X * tileSize,
                    region.Area.Y * tileSize,
                    region.Area.Width * tileSize,
                    region.Area.Height * tileSize
                );

                // Convert world pixels to screen coordinates
                Point screenTopLeft = camera.WorldToScreen(worldRect.Location.ToVector2(), canvasBounds);
                Point screenBottomRight = camera.WorldToScreen(new Vector2(worldRect.Right, worldRect.Bottom), canvasBounds);

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
                    string safeLabel = FontUtil.SanitizeForFont(font, label);
                    spriteBatch.DrawString(font, safeLabel, screenTopLeft.ToVector2() + new Vector2(4, 4), Color.White);
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

            // Draw all visible tiles (respecting visibility toggles)
            for (int layerIndex = 0; layerIndex < context.MapDefinition.TileLayers.Count; layerIndex++)
            {
                // Check visibility
                bool isVisible = layerIndex < context.TileLayerVisible.Length && context.TileLayerVisible[layerIndex];
                bool isActive = context.ActiveLayerKind == EditableLayerKind.Tiles && context.ActiveTileLayerIndex == layerIndex;
                
                // In overlay mode, show non-active layers with alpha
                Color drawColor = Color.White;
                if (context.ShowAllLayersOverlay && !isActive)
                {
                    drawColor = new Color(255, 255, 255, 90); // 35% alpha
                }
                
                if (!isVisible && !context.ShowAllLayersOverlay) continue;

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

                        // Try texture first, fallback to color
                        Texture2D tileTexture = GetTileTexture(tileId, context);
                        if (tileTexture != null)
                        {
                            spriteBatch.Draw(tileTexture, screenRect, drawColor);
                        }
                        else
                        {
                            Color fallbackColor = GetTileFallbackColor(tileId);
                            spriteBatch.Draw(pixelTexture, screenRect, fallbackColor * (drawColor.A / 255f));
                        }

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

            // Draw all visible blocks (respecting visibility toggles)
            for (int layerIndex = 0; layerIndex < context.MapDefinition.BlockLayers.Count; layerIndex++)
            {
                // Check visibility
                bool isVisible = layerIndex < context.BlockLayerVisible.Length && context.BlockLayerVisible[layerIndex];
                bool isActive = context.ActiveLayerKind == EditableLayerKind.Blocks && context.ActiveBlockLayerIndex == layerIndex;
                
                // In overlay mode, show non-active layers with alpha
                Color drawColor = Color.White;
                if (context.ShowAllLayersOverlay && !isActive)
                {
                    drawColor = new Color(255, 255, 255, 90); // 35% alpha
                }
                
                if (!isVisible && !context.ShowAllLayersOverlay) continue;

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

                        // Try variant resolver first, fallback to old method
                        string baseId = GetBlockBaseId(blockType);
                        Texture2D blockTexture = null;
                        float rotation = 0f;
                        
                        if (context.VariantResolver != null && !string.IsNullOrEmpty(baseId))
                        {
                            // Use a deterministic seed based on map dimensions for consistency
                            int worldSeed = context.MapDefinition != null
                                ? (context.MapDefinition.MapWidth * 73856093) ^ (context.MapDefinition.MapHeight * 19349663)
                                : 0;
                            var (tex, rot) = context.VariantResolver.Resolve(baseId, tx, ty, layerIndex, worldSeed);
                            blockTexture = tex;
                            rotation = rot;
                        }
                        
                        // Fallback to old method if variant resolver didn't provide texture
                        if (blockTexture == null)
                        {
                            blockTexture = GetBlockTexture(blockType, context);
                        }
                        
                        if (blockTexture != null)
                        {
                            // Draw with rotation if applicable
                            if (rotation != 0f)
                            {
                                Vector2 origin = new Vector2(screenRect.Width / 2f, screenRect.Height / 2f);
                                Vector2 position = new Vector2(screenRect.X + origin.X, screenRect.Y + origin.Y);
                                spriteBatch.Draw(blockTexture, position, null, drawColor, rotation, origin, 1f, SpriteEffects.None, 0f);
                            }
                            else
                            {
                                spriteBatch.Draw(blockTexture, screenRect, drawColor);
                            }
                        }
                        else
                        {
                            Color fallbackColor = GetBlockFallbackColor(blockType);
                            spriteBatch.Draw(pixelTexture, screenRect, fallbackColor * (drawColor.A / 255f));
                        }

                        blocksDrawn++;
                    }
                }
            }
        }

        /// <summary>
        /// Draws ITEMS from ChunkedTileMap (ItemsLow and ItemsHigh)
        /// </summary>
        public void DrawItems(SpriteBatch spriteBatch, Texture2D pixelTexture, EditorContext context, EditorCameraController camera, Rectangle canvasBounds, int layerIndex)
        {
            if (context.Map == null || context.MapDefinition == null) return;
            if (layerIndex < 0 || layerIndex >= context.MapDefinition.ItemLayers.Count) return;

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

            // Check visibility
            bool isVisible = layerIndex < context.ItemLayerVisible.Length && context.ItemLayerVisible[layerIndex];
            bool isActive = (context.ActiveLayerKind == EditableLayerKind.ItemsLow && layerIndex == 0) ||
                           (context.ActiveLayerKind == EditableLayerKind.ItemsHigh && layerIndex == 1);
            
            // In overlay mode, show non-active layers with alpha
            Color drawColor = Color.White;
            if (context.ShowAllLayersOverlay && !isActive)
            {
                drawColor = new Color(255, 255, 255, 90); // 35% alpha
            }
            
            if (!isVisible && !context.ShowAllLayersOverlay) return;

            for (int ty = startY; ty < endY; ty++)
            {
                for (int tx = startX; tx < endX; tx++)
                {
                    ItemType itemType = context.Map.GetItemAtTile(tx, ty, layerIndex);
                    if (itemType == ItemType.Empty) continue;

                    Vector2 worldPos = new Vector2(tx * tileSize, ty * tileSize);
                    Point screenTopLeft = camera.WorldToScreen(worldPos, canvasBounds);
                    Point screenBottomRight = camera.WorldToScreen(worldPos + new Vector2(tileSize, tileSize), canvasBounds);

                    Rectangle screenRect = new Rectangle(
                        screenTopLeft.X,
                        screenTopLeft.Y,
                        screenBottomRight.X - screenTopLeft.X,
                        screenBottomRight.Y - screenTopLeft.Y
                    );

                    // Try texture first, fallback to color
                    Texture2D itemTexture = GetItemTexture(itemType, context);
                    if (itemTexture != null)
                    {
                        spriteBatch.Draw(itemTexture, screenRect, drawColor);
                    }
                    else
                    {
                        Color fallbackColor = GetItemFallbackColor(itemType);
                        spriteBatch.Draw(pixelTexture, screenRect, fallbackColor * (drawColor.A / 255f));
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

        private Texture2D GetTileTexture(int tileId, EditorContext context)
        {
            if (context.TextureManager == null) return null;
            
            string textureKey = tileId switch
            {
                1 => "grass",
                2 => "dirt",
                3 => "stone",
                4 => "floor",
                _ => null
            };
            
            return textureKey != null ? context.TextureManager.GetTexture(textureKey) : null;
        }

        private string GetBlockBaseId(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Wall => "wall",
                BlockType.Crate => "crate",
                BlockType.Tree => "tree",
                BlockType.Rock => "rock",
                _ => null
            };
        }

        private Texture2D GetBlockTexture(BlockType blockType, EditorContext context)
        {
            // Fallback method - try to get texture by block type name
            string textureKey = GetBlockBaseId(blockType);
            return textureKey != null && context.TextureManager != null 
                ? context.TextureManager.GetTexture(textureKey) 
                : null;
        }

        private Texture2D GetItemTexture(ItemType itemType, EditorContext context)
        {
            if (context.TextureManager == null) return null;
            
            string textureKey = itemType switch
            {
                ItemType.Hammer => "hammer",
                ItemType.Apple => "apple",
                ItemType.WoodPickup => "wood",
                ItemType.GoldPickup => "gold",
                ItemType.Brain => "brain",
                ItemType.Gun => "gun",
                _ => null
            };
            
            return textureKey != null ? context.TextureManager.GetTexture(textureKey) : null;
        }

        private Color GetItemFallbackColor(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Hammer => new Color(139, 69, 19),    // Brown
                ItemType.Apple => new Color(255, 0, 0),       // Red
                ItemType.WoodPickup => new Color(160, 82, 45), // Sienna
                ItemType.GoldPickup => new Color(255, 215, 0), // Gold
                ItemType.Brain => new Color(255, 192, 203),    // Pink
                ItemType.Gun => new Color(105, 105, 105),      // Dim gray
                _ => Color.White
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

