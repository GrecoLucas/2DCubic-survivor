using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Renders small preview thumbnails of maps for the menu.
    /// Caches previews to avoid regenerating constantly.
    /// </summary>
    public class MapPreviewRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture2D _pixelTexture;
        private readonly Dictionary<string, Texture2D> _previewCache = new();

        public const int PreviewSize = 220; // Square preview size

        public MapPreviewRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            
            // Create pixel texture for fallback rendering
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Gets or generates a preview texture for a map.
        /// </summary>
        public Texture2D GetPreview(string mapPath)
        {
            // Check cache first
            if (_previewCache.TryGetValue(mapPath, out var cached))
            {
                return cached;
            }

            // Generate new preview
            try
            {
                var preview = GeneratePreview(mapPath);
                _previewCache[mapPath] = preview;
                return preview;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapPreviewRenderer] Failed to generate preview for {mapPath}: {ex.Message}");
                return GenerateErrorPreview();
            }
        }

        /// <summary>
        /// Clears the preview cache (call when maps change).
        /// </summary>
        public void ClearCache()
        {
            foreach (var texture in _previewCache.Values)
            {
                texture?.Dispose();
            }
            _previewCache.Clear();
            Console.WriteLine("[MapPreviewRenderer] Cache cleared");
        }

        /// <summary>
        /// Removes a specific preview from cache.
        /// </summary>
        public void InvalidatePreview(string mapPath)
        {
            if (_previewCache.TryGetValue(mapPath, out var texture))
            {
                texture?.Dispose();
                _previewCache.Remove(mapPath);
            }
        }

        private Texture2D GeneratePreview(string mapPath)
        {
            // Load map definition
            var mapDef = MapLoader.Load(mapPath);
            if (mapDef == null)
            {
                return GenerateErrorPreview();
            }

            // Create render target
            var renderTarget = new RenderTarget2D(
                _graphicsDevice,
                PreviewSize,
                PreviewSize,
                false,
                SurfaceFormat.Color,
                DepthFormat.None
            );

            _graphicsDevice.SetRenderTarget(renderTarget);
            _graphicsDevice.Clear(new Color(20, 20, 20)); // Dark background

            var spriteBatch = new SpriteBatch(_graphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Calculate sampling - show center portion of map
            int mapWidth = mapDef.MapWidth;
            int mapHeight = mapDef.MapHeight;
            
            // Sample a 64x64 tile area from center (or entire map if smaller)
            int sampleSize = Math.Min(64, Math.Min(mapWidth, mapHeight));
            int startX = (mapWidth - sampleSize) / 2;
            int startY = (mapHeight - sampleSize) / 2;
            
            float tileRenderSize = (float)PreviewSize / sampleSize;

            // Render tiles
            var chunkedMap = new ChunkedTileMap(mapDef);
            
            for (int ty = 0; ty < sampleSize; ty++)
            {
                for (int tx = 0; tx < sampleSize; tx++)
                {
                    int mapX = startX + tx;
                    int mapY = startY + ty;

                    if (mapX < 0 || mapY < 0 || mapX >= mapWidth || mapY >= mapHeight)
                        continue;

                    // Draw tile (layer 0)
                    int tileId = chunkedMap.GetTileAt(mapX, mapY, 0);
                    if (tileId != 0)
                    {
                        Color tileColor = GetTileFallbackColor(tileId);
                        Rectangle destRect = new Rectangle(
                            (int)(tx * tileRenderSize),
                            (int)(ty * tileRenderSize),
                            (int)Math.Ceiling(tileRenderSize),
                            (int)Math.Ceiling(tileRenderSize)
                        );
                        spriteBatch.Draw(_pixelTexture, destRect, tileColor);
                    }

                    // Draw block (layer 0)
                    var blockType = chunkedMap.GetBlockAtTile(mapX, mapY, 0);
                    if (blockType != BlockType.Empty)
                    {
                        Color blockColor = GetBlockFallbackColor(blockType);
                        Rectangle destRect = new Rectangle(
                            (int)(tx * tileRenderSize),
                            (int)(ty * tileRenderSize),
                            (int)Math.Ceiling(tileRenderSize),
                            (int)Math.Ceiling(tileRenderSize)
                        );
                        spriteBatch.Draw(_pixelTexture, destRect, blockColor);
                    }
                }
            }

            spriteBatch.End();
            _graphicsDevice.SetRenderTarget(null);

            return renderTarget;
        }

        private Texture2D GenerateErrorPreview()
        {
            var renderTarget = new RenderTarget2D(
                _graphicsDevice,
                PreviewSize,
                PreviewSize
            );

            _graphicsDevice.SetRenderTarget(renderTarget);
            _graphicsDevice.Clear(Color.DarkRed);

            var spriteBatch = new SpriteBatch(_graphicsDevice);
            spriteBatch.Begin();
            
            // Draw X pattern
            for (int i = 0; i < PreviewSize; i++)
            {
                spriteBatch.Draw(_pixelTexture, new Rectangle(i, i, 2, 2), Color.White);
                spriteBatch.Draw(_pixelTexture, new Rectangle(i, PreviewSize - i, 2, 2), Color.White);
            }
            
            spriteBatch.End();
            _graphicsDevice.SetRenderTarget(null);

            return renderTarget;
        }

        private Color GetTileFallbackColor(int tileId)
        {
            return tileId switch
            {
                1 => new Color(50, 200, 50, 255),    // Grass - green
                2 => new Color(100, 100, 255, 255),  // Water - blue
                3 => new Color(200, 180, 140, 255),  // Sand - tan
                _ => new Color(80, 80, 80, 255)      // Unknown - dark gray
            };
        }

        private Color GetBlockFallbackColor(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Wall => new Color(120, 120, 120, 255),   // Gray
                BlockType.Crate => new Color(180, 120, 60, 255),   // Brown
                BlockType.Tree => new Color(34, 140, 34, 255),     // Green
                BlockType.Rock => new Color(100, 100, 120, 255),   // Blue-gray
                _ => Color.Transparent
            };
        }

        public void Dispose()
        {
            ClearCache();
            _pixelTexture?.Dispose();
        }
    }
}

