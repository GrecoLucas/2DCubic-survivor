using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Runtime representation of a chunked tile map.
    /// Provides efficient access to tiles and blocks with lazy chunk loading.
    /// </summary>
    public sealed class ChunkedTileMap
    {
        public MapDefinition Definition { get; }
        public int ChunkSize => Definition.ChunkSize;
        public int TileSize => Definition.TileSize;
        public int MapWidthInPixels => Definition.MapWidth * Definition.TileSize;
        public int MapHeightInPixels => Definition.MapHeight * Definition.TileSize;

        public ChunkedTileMap(MapDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        #region Block Operations

        /// <summary>
        /// Gets the block type at a specific tile coordinate.
        /// </summary>
        /// <param name="tx">Tile X coordinate.</param>
        /// <param name="ty">Tile Y coordinate.</param>
        /// <param name="layerIndex">Block layer index (default 0).</param>
        /// <returns>The block type at that position, or Empty if out of bounds.</returns>
        public BlockType GetBlockAtTile(int tx, int ty, int layerIndex = 0)
        {
            if (!IsValidTileCoord(tx, ty) || layerIndex < 0 || layerIndex >= Definition.BlockLayers.Count)
                return BlockType.Empty;

            var layer = Definition.BlockLayers[layerIndex];
            var (chunkCoord, localX, localY) = TileToChunkCoords(tx, ty);
            string chunkKey = ChunkKeyToString(chunkCoord);

            if (!layer.Chunks.TryGetValue(chunkKey, out var chunkData))
                return BlockType.Empty;

            if (chunkData?.Blocks == null)
                return BlockType.Empty;

            return chunkData.Blocks[localX, localY];
        }

        /// <summary>
        /// Sets the block type at a specific tile coordinate.
        /// Creates chunk if it doesn't exist.
        /// </summary>
        /// <param name="tx">Tile X coordinate.</param>
        /// <param name="ty">Tile Y coordinate.</param>
        /// <param name="type">Block type to set.</param>
        /// <param name="layerIndex">Block layer index (default 0).</param>
        public void SetBlockAtTile(int tx, int ty, BlockType type, int layerIndex = 0)
        {
            if (!IsValidTileCoord(tx, ty) || layerIndex < 0 || layerIndex >= Definition.BlockLayers.Count)
                return;

            var layer = Definition.BlockLayers[layerIndex];
            var (chunkCoord, localX, localY) = TileToChunkCoords(tx, ty);
            string chunkKey = ChunkKeyToString(chunkCoord);

            if (!layer.Chunks.TryGetValue(chunkKey, out var chunkData))
            {
                // Create new chunk
                chunkData = new ChunkBlockData
                {
                    Blocks = new BlockType[ChunkSize, ChunkSize]
                };
                layer.Chunks[chunkKey] = chunkData;
            }

            if (chunkData.Blocks == null)
            {
                chunkData.Blocks = new BlockType[ChunkSize, ChunkSize];
            }

            chunkData.Blocks[localX, localY] = type;
        }

        /// <summary>
        /// Gets the block type at a world position (in pixels).
        /// </summary>
        public BlockType GetBlockAtWorldPos(Vector2 worldPos, int layerIndex = 0)
        {
            int tx = (int)(worldPos.X / TileSize);
            int ty = (int)(worldPos.Y / TileSize);
            return GetBlockAtTile(tx, ty, layerIndex);
        }

        #endregion

        #region Tile Operations

        /// <summary>
        /// Gets the tile ID at a specific tile coordinate.
        /// </summary>
        /// <param name="tx">Tile X coordinate.</param>
        /// <param name="ty">Tile Y coordinate.</param>
        /// <param name="layerIndex">Tile layer index (default 0).</param>
        /// <returns>The tile ID at that position, or 0 if empty/out of bounds.</returns>
        public int GetTileAt(int tx, int ty, int layerIndex = 0)
        {
            if (!IsValidTileCoord(tx, ty) || layerIndex < 0 || layerIndex >= Definition.TileLayers.Count)
                return 0;

            var layer = Definition.TileLayers[layerIndex];
            var (chunkCoord, localX, localY) = TileToChunkCoords(tx, ty);
            string chunkKey = ChunkKeyToString(chunkCoord);

            if (!layer.Chunks.TryGetValue(chunkKey, out var chunkData))
                return 0;

            if (chunkData?.Tiles == null)
                return 0;

            return chunkData.Tiles[localX, localY];
        }

        /// <summary>
        /// Sets the tile ID at a specific tile coordinate.
        /// Creates chunk if it doesn't exist.
        /// </summary>
        /// <param name="tx">Tile X coordinate.</param>
        /// <param name="ty">Tile Y coordinate.</param>
        /// <param name="tileId">Tile ID to set (0 = empty).</param>
        /// <param name="layerIndex">Tile layer index (default 0).</param>
        public void SetTileAt(int tx, int ty, int tileId, int layerIndex = 0)
        {
            if (!IsValidTileCoord(tx, ty) || layerIndex < 0 || layerIndex >= Definition.TileLayers.Count)
                return;

            var layer = Definition.TileLayers[layerIndex];
            var (chunkCoord, localX, localY) = TileToChunkCoords(tx, ty);
            string chunkKey = ChunkKeyToString(chunkCoord);

            if (!layer.Chunks.TryGetValue(chunkKey, out var chunkData))
            {
                // Create new chunk
                chunkData = new ChunkTileData
                {
                    Tiles = new int[ChunkSize, ChunkSize]
                };
                layer.Chunks[chunkKey] = chunkData;
            }

            if (chunkData.Tiles == null)
            {
                chunkData.Tiles = new int[ChunkSize, ChunkSize];
            }

            chunkData.Tiles[localX, localY] = tileId;
        }

        #endregion

        #region Visible Chunk Queries

        /// <summary>
        /// Gets all visible tile chunks within the camera view.
        /// </summary>
        /// <param name="worldView">World-space rectangle representing camera view.</param>
        /// <param name="layerIndex">Tile layer index.</param>
        /// <returns>Enumerable of chunk coordinates and their data.</returns>
        public IEnumerable<(Point chunkCoord, ChunkTileData data)> GetVisibleTileChunks(Rectangle worldView, int layerIndex = 0)
        {
            if (layerIndex < 0 || layerIndex >= Definition.TileLayers.Count)
                yield break;

            var layer = Definition.TileLayers[layerIndex];

            // Convert world rect to tile coords
            int minTx = Math.Max(0, worldView.Left / TileSize);
            int minTy = Math.Max(0, worldView.Top / TileSize);
            int maxTx = Math.Min(Definition.MapWidth - 1, worldView.Right / TileSize);
            int maxTy = Math.Min(Definition.MapHeight - 1, worldView.Bottom / TileSize);

            // Convert to chunk coords
            int minCx = minTx / ChunkSize;
            int minCy = minTy / ChunkSize;
            int maxCx = maxTx / ChunkSize;
            int maxCy = maxTy / ChunkSize;

            for (int cy = minCy; cy <= maxCy; cy++)
            {
                for (int cx = minCx; cx <= maxCx; cx++)
                {
                    Point chunkCoord = new Point(cx, cy);
                    string chunkKey = ChunkKeyToString(chunkCoord);

                    if (layer.Chunks.TryGetValue(chunkKey, out var chunkData) && chunkData?.Tiles != null)
                    {
                        yield return (chunkCoord, chunkData);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all visible block chunks within the camera view.
        /// </summary>
        /// <param name="worldView">World-space rectangle representing camera view.</param>
        /// <param name="layerIndex">Block layer index.</param>
        /// <returns>Enumerable of chunk coordinates and their data.</returns>
        public IEnumerable<(Point chunkCoord, ChunkBlockData data)> GetVisibleBlockChunks(Rectangle worldView, int layerIndex = 0)
        {
            if (layerIndex < 0 || layerIndex >= Definition.BlockLayers.Count)
                yield break;

            var layer = Definition.BlockLayers[layerIndex];

            // Convert world rect to tile coords
            int minTx = Math.Max(0, worldView.Left / TileSize);
            int minTy = Math.Max(0, worldView.Top / TileSize);
            int maxTx = Math.Min(Definition.MapWidth - 1, worldView.Right / TileSize);
            int maxTy = Math.Min(Definition.MapHeight - 1, worldView.Bottom / TileSize);

            // Convert to chunk coords
            int minCx = minTx / ChunkSize;
            int minCy = minTy / ChunkSize;
            int maxCx = maxTx / ChunkSize;
            int maxCy = maxTy / ChunkSize;

            for (int cy = minCy; cy <= maxCy; cy++)
            {
                for (int cx = minCx; cx <= maxCx; cx++)
                {
                    Point chunkCoord = new Point(cx, cy);
                    string chunkKey = ChunkKeyToString(chunkCoord);

                    if (layer.Chunks.TryGetValue(chunkKey, out var chunkData) && chunkData?.Blocks != null)
                    {
                        yield return (chunkCoord, chunkData);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all blocks within a specific tile rectangle.
        /// Used for streaming block entities.
        /// </summary>
        /// <param name="tileRect">Rectangle in tile coordinates.</param>
        /// <param name="layerIndex">Block layer index.</param>
        /// <returns>Enumerable of tile coordinates and block types.</returns>
        public IEnumerable<(int tx, int ty, BlockType blockType)> GetBlocksInTileRect(Rectangle tileRect, int layerIndex = 0)
        {
            if (layerIndex < 0 || layerIndex >= Definition.BlockLayers.Count)
                yield break;

            int minTx = Math.Max(0, tileRect.Left);
            int minTy = Math.Max(0, tileRect.Top);
            int maxTx = Math.Min(Definition.MapWidth - 1, tileRect.Right);
            int maxTy = Math.Min(Definition.MapHeight - 1, tileRect.Bottom);

            for (int ty = minTy; ty <= maxTy; ty++)
            {
                for (int tx = minTx; tx <= maxTx; tx++)
                {
                    BlockType block = GetBlockAtTile(tx, ty, layerIndex);
                    if (block != BlockType.Empty)
                    {
                        yield return (tx, ty, block);
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts tile coordinates to chunk coordinates and local tile coordinates.
        /// </summary>
        private (Point chunkCoord, int localX, int localY) TileToChunkCoords(int tx, int ty)
        {
            int cx = tx / ChunkSize;
            int cy = ty / ChunkSize;
            int lx = tx % ChunkSize;
            int ly = ty % ChunkSize;

            return (new Point(cx, cy), lx, ly);
        }

        /// <summary>
        /// Converts a Point to a string key for dictionary storage.
        /// </summary>
        private string ChunkKeyToString(Point p)
        {
            return $"{p.X},{p.Y}";
        }

        /// <summary>
        /// Checks if a tile coordinate is within map bounds.
        /// </summary>
        private bool IsValidTileCoord(int tx, int ty)
        {
            return tx >= 0 && ty >= 0 && tx < Definition.MapWidth && ty < Definition.MapHeight;
        }

        /// <summary>
        /// Converts world position to tile coordinates.
        /// </summary>
        public Point WorldToTileCoords(Vector2 worldPos)
        {
            return new Point((int)(worldPos.X / TileSize), (int)(worldPos.Y / TileSize));
        }

        /// <summary>
        /// Converts tile coordinates to world position (top-left corner of tile).
        /// </summary>
        public Vector2 TileToWorldPos(int tx, int ty)
        {
            return new Vector2(tx * TileSize, ty * TileSize);
        }

        #endregion
    }
}

