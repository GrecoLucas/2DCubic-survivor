using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Final map definition supporting multi-layer chunked maps with regions.
    /// Designed for huge, streamable worlds with data-driven spawns and obstacles.
    /// </summary>
    public sealed class MapDefinition
    {
        /// <summary>
        /// Total map width in tiles.
        /// </summary>
        public int MapWidth { get; set; } = 256;

        /// <summary>
        /// Total map height in tiles.
        /// </summary>
        public int MapHeight { get; set; } = 256;

        /// <summary>
        /// Number of tiles per chunk (for both width and height).
        /// </summary>
        public int ChunkSize { get; set; } = 64;

        /// <summary>
        /// Size of each tile in pixels.
        /// </summary>
        public int TileSize { get; set; } = 128;

        // ============================================================
        // COMPATIBILITY ALIASES (Read-Only, Not Serialized)
        // ============================================================
        
        /// <summary>
        /// Alias for MapWidth (compatibility).
        /// </summary>
        [JsonIgnore]
        public int MapWidthTiles => MapWidth;

        /// <summary>
        /// Alias for MapWidth (compatibility).
        /// </summary>
        [JsonIgnore]
        public int WidthTiles => MapWidth;

        /// <summary>
        /// Alias for MapHeight (compatibility).
        /// </summary>
        [JsonIgnore]
        public int MapHeightTiles => MapHeight;

        /// <summary>
        /// Alias for MapHeight (compatibility).
        /// </summary>
        [JsonIgnore]
        public int HeightTiles => MapHeight;

        /// <summary>
        /// Alias for TileSize (compatibility).
        /// </summary>
        [JsonIgnore]
        public int TileSizePx => TileSize;

        /// <summary>
        /// Alias for ChunkSize (compatibility).
        /// </summary>
        [JsonIgnore]
        public int ChunkSizeTiles => ChunkSize;

        // ============================================================

        /// <summary>
        /// List of tile layers (ground, decoration, etc.).
        /// </summary>
        public List<TileLayerDefinition> TileLayers { get; set; } = new();

        /// <summary>
        /// List of block layers (walls, crates, trees, etc.).
        /// </summary>
        public List<BlockLayerDefinition> BlockLayers { get; set; } = new();

        /// <summary>
        /// List of regions for spawns, safe zones, biomes, etc.
        /// </summary>
        public List<RegionDefinition> Regions { get; set; } = new();
    }

    /// <summary>
    /// Defines a tile layer (non-collidable, typically ground/background).
    /// </summary>
    public sealed class TileLayerDefinition
    {
        /// <summary>
        /// Name of this layer (e.g., "Ground", "Decoration").
        /// </summary>
        public string Name { get; set; } = "Ground";

        /// <summary>
        /// Whether this layer should contribute to collision (usually false for ground).
        /// </summary>
        public bool IsCollisionLayer { get; set; } = false;

        /// <summary>
        /// Chunked tile data indexed by chunk coordinate (cx, cy).
        /// Key format: "cx,cy" (e.g., "0,0", "1,2").
        /// </summary>
        public Dictionary<string, ChunkTileData> Chunks { get; set; } = new();
    }

    /// <summary>
    /// Defines a block layer (collidable obstacles like walls, trees, crates).
    /// </summary>
    public sealed class BlockLayerDefinition
    {
        /// <summary>
        /// Name of this layer (e.g., "Blocks", "Obstacles").
        /// </summary>
        public string Name { get; set; } = "Blocks";

        /// <summary>
        /// Whether blocks in this layer should collide (usually true).
        /// </summary>
        public bool IsCollisionLayer { get; set; } = true;

        /// <summary>
        /// Chunked block data indexed by chunk coordinate (cx, cy).
        /// Key format: "cx,cy" (e.g., "0,0", "1,2").
        /// </summary>
        public Dictionary<string, ChunkBlockData> Chunks { get; set; } = new();
    }

    /// <summary>
    /// Tile data for a single chunk.
    /// </summary>
    public sealed class ChunkTileData
    {
        /// <summary>
        /// 2D array of tile IDs. Size is [ChunkSize, ChunkSize].
        /// 0 means empty/no tile.
        /// </summary>
        [JsonConverter(typeof(Array2DJsonConverter<int>))]
        public int[,] Tiles { get; set; }
    }

    /// <summary>
    /// Block data for a single chunk.
    /// </summary>
    public sealed class ChunkBlockData
    {
        /// <summary>
        /// 2D array of block types. Size is [ChunkSize, ChunkSize].
        /// </summary>
        [JsonConverter(typeof(Array2DJsonConverter<BlockType>))]
        public BlockType[,] Blocks { get; set; }
    }

    /// <summary>
    /// Types of blocks that can exist in the world.
    /// </summary>
    public enum BlockType
    {
        Empty = 0,
        Wall = 1,
        Crate = 2,
        Tree = 3,
        Rock = 4
        // Extensible: add Door, Water, Chest, etc. as needed
    }

    /// <summary>
    /// Defines a region in the map for spawning, safe zones, biomes, etc.
    /// Unifies all spawn/area data in a data-driven way.
    /// </summary>
    public sealed class RegionDefinition
    {
        /// <summary>
        /// Unique identifier for this region.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of region (player spawn, enemy spawn, etc.).
        /// </summary>
        public RegionType Type { get; set; }

        /// <summary>
        /// World-space rectangle (in pixels) defining the region bounds.
        /// </summary>
        public Rectangle Area { get; set; }

        /// <summary>
        /// Optional metadata for configuring behavior.
        /// Examples: "maxEnemies", "intervalSeconds", "biome", "maxActive", etc.
        /// </summary>
        public Dictionary<string, string> Meta { get; set; } = new();
    }

    /// <summary>
    /// Types of regions that can be defined in a map.
    /// </summary>
    public enum RegionType
    {
        PlayerSpawn,
        EnemySpawn,
        TreeSpawn,
        WoodSpawn,
        GoldSpawn,
        SafeZone,
        Biome
        // Extensible: add QuestZone, TriggerZone, etc.
    }
}

