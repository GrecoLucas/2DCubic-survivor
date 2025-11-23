using System;
using CubeSurvivor.Core;
using CubeSurvivor.Game.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Systems.Rendering
{
    /// <summary>
    /// Renders the chunked tile map with camera culling.
    /// Only draws visible tiles and blocks for maximum performance.
    /// </summary>
    public sealed class MapRenderSystem : GameSystem
    {
        private readonly ChunkedTileMap _map;
        private readonly SpriteBatch _spriteBatch;
        private readonly CameraService _cameraService;
        private readonly TextureManager _textureManager;
        private Texture2D _pixelTexture;

        // Block colors for visual representation
        private readonly Color _wallColor = new Color(80, 80, 80);      // Gray
        private readonly Color _crateColor = new Color(139, 69, 19);    // Brown
        private readonly Color _treeColor = new Color(34, 139, 34);     // Forest green
        private readonly Color _rockColor = new Color(105, 105, 105);   // Dim gray

        public MapRenderSystem(ChunkedTileMap map, SpriteBatch spriteBatch, CameraService cameraService, TextureManager textureManager)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
            
            // Initialize pixel texture in constructor since we have access to GraphicsDevice
            _pixelTexture = new Texture2D(_spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public override void Update(GameTime gameTime)
        {
            // No update logic needed - this is a render-only system
        }

        /// <summary>
        /// Draws the map using the camera transform.
        /// Should be called during the draw phase, not update.
        /// </summary>
        public void Draw(Matrix cameraTransform)
        {
            // Calculate visible world rectangle from camera
            Rectangle visibleWorldRect = CalculateVisibleWorldRect(cameraTransform);

            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                transformMatrix: cameraTransform
            );

            // Draw all tile layers first (background)
            for (int layerIndex = 0; layerIndex < _map.Definition.TileLayers.Count; layerIndex++)
            {
                DrawTileLayer(visibleWorldRect, layerIndex);
            }

            // Draw all block layers on top
            for (int layerIndex = 0; layerIndex < _map.Definition.BlockLayers.Count; layerIndex++)
            {
                DrawBlockLayer(visibleWorldRect, layerIndex);
            }

            _spriteBatch.End();
        }

        private void DrawTileLayer(Rectangle visibleWorldRect, int layerIndex)
        {
            var layer = _map.Definition.TileLayers[layerIndex];
            
            foreach (var (chunkCoord, chunkData) in _map.GetVisibleTileChunks(visibleWorldRect, layerIndex))
            {
                // Calculate chunk world position
                int chunkWorldX = chunkCoord.X * _map.ChunkSize * _map.TileSize;
                int chunkWorldY = chunkCoord.Y * _map.ChunkSize * _map.TileSize;

                // Draw each tile in the chunk
                for (int ly = 0; ly < _map.ChunkSize; ly++)
                {
                    for (int lx = 0; lx < _map.ChunkSize; lx++)
                    {
                        int tileId = chunkData.Tiles[lx, ly];
                        if (tileId == 0)
                            continue; // Skip empty tiles

                        // Calculate world position for this tile
                        int worldX = chunkWorldX + lx * _map.TileSize;
                        int worldY = chunkWorldY + ly * _map.TileSize;

                        // For now, draw tiles as colored rectangles
                        // TODO: Replace with actual tile sprite rendering from tileset
                        Color tileColor = GetTileColor(tileId);
                        Rectangle destRect = new Rectangle(worldX, worldY, _map.TileSize, _map.TileSize);
                        _spriteBatch.Draw(_pixelTexture, destRect, tileColor);
                    }
                }
            }
        }

        private void DrawBlockLayer(Rectangle visibleWorldRect, int layerIndex)
        {
            var layer = _map.Definition.BlockLayers[layerIndex];

            foreach (var (chunkCoord, chunkData) in _map.GetVisibleBlockChunks(visibleWorldRect, layerIndex))
            {
                // Calculate chunk world position
                int chunkWorldX = chunkCoord.X * _map.ChunkSize * _map.TileSize;
                int chunkWorldY = chunkCoord.Y * _map.ChunkSize * _map.TileSize;

                // Draw each block in the chunk
                for (int ly = 0; ly < _map.ChunkSize; ly++)
                {
                    for (int lx = 0; lx < _map.ChunkSize; lx++)
                    {
                        BlockType blockType = chunkData.Blocks[lx, ly];
                        if (blockType == BlockType.Empty)
                            continue; // Skip empty blocks

                        // Calculate world position for this block
                        int worldX = chunkWorldX + lx * _map.TileSize;
                        int worldY = chunkWorldY + ly * _map.TileSize;

                        // Draw block as colored rectangle
                        Color blockColor = GetBlockColor(blockType);
                        Rectangle destRect = new Rectangle(worldX, worldY, _map.TileSize, _map.TileSize);
                        _spriteBatch.Draw(_pixelTexture, destRect, blockColor);

                        // Add a darker border for better visibility
                        DrawBorder(destRect, blockColor * 0.7f);
                    }
                }
            }
        }

        private void DrawBorder(Rectangle rect, Color color)
        {
            int borderWidth = 2;
            
            // Top
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), color);
            // Bottom
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Bottom - borderWidth, rect.Width, borderWidth), color);
            // Left
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), color);
            // Right
            _spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Right - borderWidth, rect.Y, borderWidth, rect.Height), color);
        }

        private Color GetBlockColor(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Wall => _wallColor,
                BlockType.Crate => _crateColor,
                BlockType.Tree => _treeColor,
                BlockType.Rock => _rockColor,
                _ => Color.White
            };
        }

        private Color GetTileColor(int tileId)
        {
            // Simple color mapping for now
            // TODO: Replace with actual tileset texture atlas
            return tileId switch
            {
                1 => new Color(34, 139, 34),    // Grass green
                2 => new Color(139, 90, 43),    // Dirt brown
                3 => new Color(128, 128, 128),  // Stone gray
                _ => new Color(50, 50, 50)      // Default dark
            };
        }

        private Rectangle CalculateVisibleWorldRect(Matrix cameraTransform)
        {
            // Invert camera transform to get world bounds
            Matrix inverted = Matrix.Invert(cameraTransform);

            // Screen corners in screen space
            Vector2 topLeft = Vector2.Transform(Vector2.Zero, inverted);
            Vector2 bottomRight = Vector2.Transform(
                new Vector2(_cameraService.ScreenWidth, _cameraService.ScreenHeight), 
                inverted
            );

            // Add padding to avoid pop-in
            int padding = _map.TileSize * 2;
            
            int x = (int)topLeft.X - padding;
            int y = (int)topLeft.Y - padding;
            int width = (int)(bottomRight.X - topLeft.X) + padding * 2;
            int height = (int)(bottomRight.Y - topLeft.Y) + padding * 2;

            // Clamp to map bounds
            x = Math.Max(0, x);
            y = Math.Max(0, y);
            width = Math.Min(_map.MapWidthInPixels - x, width);
            height = Math.Min(_map.MapHeightInPixels - y, height);

            return new Rectangle(x, y, width, height);
        }
    }
}

