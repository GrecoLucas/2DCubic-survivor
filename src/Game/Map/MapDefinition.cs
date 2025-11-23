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
        /// Should match player sprite size (default: 32px).
        /// </summary>
        public int TileSize { get; set; } = 32;

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
        /// List of item layers. Exactly 2 layers: [0] = ItemsLow, [1] = ItemsHigh.
        /// ItemsLow renders below blocks, ItemsHigh renders above blocks.
        /// </summary>
        public List<ItemLayerDefinition> ItemLayers { get; set; } = new();

        /// <summary>
        /// List of block layers (walls, crates, trees, etc.).
        /// </summary>
        public List<BlockLayerDefinition> BlockLayers { get; set; } = new();

        /// <summary>
        /// List of regions for spawns, safe zones, biomes, etc.
        /// </summary>
        public List<RegionDefinition> Regions { get; set; } = new();

        /// <summary>
        /// List of items placed directly on the map (not spawned via regions).
        /// </summary>
        public List<PlacedItemDefinition> PlacedItems { get; set; } = new();
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
    /// Defines an item layer (pickups, collectibles).
    /// Items never collide, they are just visual/collectible entities.
    /// </summary>
    public sealed class ItemLayerDefinition
    {
        /// <summary>
        /// Name of this layer (e.g., "ItemsLow", "ItemsHigh").
        /// </summary>
        public string Name { get; set; } = "Items";

        /// <summary>
        /// Chunked item data indexed by chunk coordinate (cx, cy).
        /// Key format: "cx,cy" (e.g., "0,0", "1,2").
        /// </summary>
        public Dictionary<string, ChunkItemData> Chunks { get; set; } = new();
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
    /// Item data for a single chunk.
    /// </summary>
    public sealed class ChunkItemData
    {
        /// <summary>
        /// 2D array of item types. Size is [ChunkSize, ChunkSize].
        /// 0 means empty/no item.
        /// </summary>
        [JsonConverter(typeof(Array2DJsonConverter<ItemType>))]
        public ItemType[,] Items { get; set; }
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
    /// Types of items that can be placed in item layers.
    /// </summary>
    public enum ItemType
    {
        Empty = 0,
        Hammer = 1,
        Apple = 2,
        WoodPickup = 3,
        GoldPickup = 4,
        Brain = 5,
        Gun = 6
        // Extensible: add more items as needed
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
    /// Region coordinates are in TILE UNITS, not pixels.
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
        /// Region bounds in TILE COORDINATES (not pixels).
        /// X, Y, Width, Height are all tile indices (0..MapWidth/MapHeight).
        /// </summary>
        public Rectangle Area { get; set; }

        /// <summary>
        /// Optional metadata for configuring behavior.
        /// Examples: "maxEnemies", "intervalSeconds", "biome", "maxActive", etc.
        /// </summary>
        public Dictionary<string, string> Meta { get; set; } = new();

        /// <summary>
        /// Converts tile-based Area to world pixel rectangle.
        /// </summary>
        public Rectangle ToWorldPixels(int tileSize)
        {
            return new Rectangle(
                Area.X * tileSize,
                Area.Y * tileSize,
                Area.Width * tileSize,
                Area.Height * tileSize
            );
        }
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
        AppleSpawn,
        ItemSpawn,  // Generic item spawn region (hammer, etc.)
        SafeZone,
        Biome
        // Extensible: add QuestZone, TriggerZone, etc.
    }

    /// <summary>
    /// Defines an item placed directly on the map at a specific tile position.
    /// </summary>
    public sealed class PlacedItemDefinition
    {
        /// <summary>
        /// Unique identifier for this placed item.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Item ID (e.g., "hammer", "apple", "wood", "gold").
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Tile coordinates where the item is placed.
        /// </summary>
        public Point Tile { get; set; }

        /// <summary>
        /// Amount of items (stack size).
        /// </summary>
        public int Amount { get; set; } = 1;

        /// <summary>
        /// Whether this item respawns after being picked up.
        /// </summary>
        public bool Respawns { get; set; } = false;

        /// <summary>
        /// Respawn interval in seconds (only used if Respawns = true).
        /// </summary>
        public float RespawnIntervalSeconds { get; set; } = 10f;
    }
}

