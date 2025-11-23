using System;
using System.Collections.Generic;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Streams block entities in and out based on camera position.
    /// Only spawns ECS entities for blocks near the player for collision detection.
    /// </summary>
    public sealed class BlockEntityStreamer
    {
        private readonly ChunkedTileMap _map;
        private readonly IWorldObjectFactory _factory;
        private readonly IGameWorld _world;
        private readonly CameraService _cameraService;

        // Key: (layerIndex, tx, ty) -> spawned entity
        private readonly Dictionary<(int, int, int), Entity> _spawnedBlocks = new();

        // Number of chunks beyond visible area to keep loaded
        private const int BufferChunks = 2;

        public BlockEntityStreamer(
            ChunkedTileMap map,
            IWorldObjectFactory factory,
            IGameWorld world,
            CameraService cameraService)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
        }

        /// <summary>
        /// Updates the streamer, spawning/despawning block entities as needed.
        /// Should be called each frame.
        /// </summary>
        public void Update()
        {
            // Calculate streaming rectangle (visible area + buffer)
            Rectangle streamRect = CalculateStreamingRect();

            // Convert to tile coordinates
            int minTx = Math.Max(0, streamRect.Left / _map.TileSize);
            int minTy = Math.Max(0, streamRect.Top / _map.TileSize);
            int maxTx = Math.Min(_map.Definition.MapWidth - 1, streamRect.Right / _map.TileSize);
            int maxTy = Math.Min(_map.Definition.MapHeight - 1, streamRect.Bottom / _map.TileSize);

            Rectangle tileRect = new Rectangle(minTx, minTy, maxTx - minTx, maxTy - minTy);

            // Track which blocks should exist
            HashSet<(int, int, int)> shouldExist = new HashSet<(int, int, int)>();

            // Process each collidable block layer
            for (int layerIndex = 0; layerIndex < _map.Definition.BlockLayers.Count; layerIndex++)
            {
                var layer = _map.Definition.BlockLayers[layerIndex];
                
                // Skip non-collision layers
                if (!layer.IsCollisionLayer)
                    continue;

                // Get all blocks in streaming area
                foreach (var (tx, ty, blockType) in _map.GetBlocksInTileRect(tileRect, layerIndex))
                {
                    var key = (layerIndex, tx, ty);
                    shouldExist.Add(key);

                    // If not already spawned, spawn it
                    if (!_spawnedBlocks.ContainsKey(key))
                    {
                        SpawnBlockEntity(layerIndex, tx, ty, blockType);
                    }
                }
            }

            // Despawn blocks that are no longer in range
            List<(int, int, int)> toRemove = new List<(int, int, int)>();
            foreach (var key in _spawnedBlocks.Keys)
            {
                if (!shouldExist.Contains(key))
                {
                    toRemove.Add(key);
                }
            }

            foreach (var key in toRemove)
            {
                DespawnBlockEntity(key);
            }
        }

        /// <summary>
        /// Removes a block from the map and despawns its entity if spawned.
        /// </summary>
        public void RemoveBlock(int tx, int ty, int layerIndex = 0)
        {
            // Remove from map
            _map.SetBlockAtTile(tx, ty, BlockType.Empty, layerIndex);

            // Remove entity if spawned
            var key = (layerIndex, tx, ty);
            if (_spawnedBlocks.TryGetValue(key, out var entity))
            {
                _world.RemoveEntity(entity);
                _spawnedBlocks.Remove(key);
            }
        }

        /// <summary>
        /// Clears all spawned block entities.
        /// Useful when switching maps.
        /// </summary>
        public void ClearAll()
        {
            foreach (var entity in _spawnedBlocks.Values)
            {
                _world.RemoveEntity(entity);
            }
            _spawnedBlocks.Clear();
        }

        /// <summary>
        /// Gets the number of currently spawned block entities.
        /// </summary>
        public int GetSpawnedBlockCount()
        {
            return _spawnedBlocks.Count;
        }

        private void SpawnBlockEntity(int layerIndex, int tx, int ty, BlockType blockType)
        {
            Vector2 worldPos = _map.TileToWorldPos(tx, ty);
            
            // Center position for the entity
            Vector2 centerPos = worldPos + new Vector2(_map.TileSize / 2f, _map.TileSize / 2f);

            Entity entity = blockType switch
            {
                BlockType.Wall => _factory.CreateWall(_world, centerPos, _map.TileSize, _map.TileSize),
                BlockType.Crate => _factory.CreateCrate(_world, centerPos, _map.TileSize, _map.TileSize, isDestructible: true, maxHealth: 100f),
                BlockType.Tree => CreateTree(centerPos),
                BlockType.Rock => _factory.CreateRock(_world, centerPos, _map.TileSize),
                _ => null
            };

            if (entity != null)
            {
                var key = (layerIndex, tx, ty);
                _spawnedBlocks[key] = entity;
                
                // Add MapBlockComponent so we can remove it from map when destroyed
                entity.AddComponent(new Components.MapBlockComponent(tx, ty, layerIndex));
            }
        }

        private Entity CreateTree(Vector2 centerPos)
        {
            // Create tree as a special type of obstacle
            var tree = _world.CreateEntity("Tree");

            float treeSize = _map.TileSize;

            tree.AddComponent(new Components.TransformComponent(centerPos));
            tree.AddComponent(new Components.SpriteComponent(
                new Color(34, 139, 34), // Forest green
                treeSize,
                treeSize,
                Components.RenderLayer.GroundEffects
            ));

            tree.AddComponent(new Components.ColliderComponent(treeSize, treeSize, Components.ColliderTag.Tree));
            tree.AddComponent(new Components.ObstacleComponent(
                blocksMovement: true,
                blocksBullets: true,
                isDestructible: true
            ));

            // Trees have health and can be harvested
            tree.AddComponent(new Components.HealthComponent(maxHealth: 30f));

            return tree;
        }

        private void DespawnBlockEntity((int layerIndex, int tx, int ty) key)
        {
            if (_spawnedBlocks.TryGetValue(key, out var entity))
            {
                _world.RemoveEntity(entity);
                _spawnedBlocks.Remove(key);
            }
        }

        private Rectangle CalculateStreamingRect()
        {
            // Get camera center (approximate player position)
            // For now, use center of screen as focal point
            int screenCenterX = _cameraService.ScreenWidth / 2;
            int screenCenterY = _cameraService.ScreenHeight / 2;

            // Calculate buffer size in pixels
            int bufferSize = BufferChunks * _map.ChunkSize * _map.TileSize;

            // Create streaming rectangle
            int width = _cameraService.ScreenWidth + bufferSize * 2;
            int height = _cameraService.ScreenHeight + bufferSize * 2;

            // Center on camera position (we'll need to adjust this based on actual camera transform)
            // For now, use a simple approach
            Rectangle rect = new Rectangle(
                -bufferSize,
                -bufferSize,
                width,
                height
            );

            // Clamp to map bounds
            rect.X = Math.Max(0, rect.X);
            rect.Y = Math.Max(0, rect.Y);
            rect.Width = Math.Min(_map.MapWidthInPixels - rect.X, rect.Width);
            rect.Height = Math.Min(_map.MapHeightInPixels - rect.Y, rect.Height);

            return rect;
        }
    }
}

