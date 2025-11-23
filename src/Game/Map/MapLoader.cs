using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using CubeSurvivor.Game.Map.Serialization;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Loads MapDefinition from JSON files.
    /// Handles deserialization with custom Point key conversion.
    /// </summary>
    public static class MapLoader
    {
        /// <summary>
        /// Loads a MapDefinition from a JSON file path.
        /// </summary>
        /// <param name="path">Path to the JSON file (relative or absolute).</param>
        /// <returns>The loaded map definition, or null if loading fails.</returns>
        public static MapDefinition Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[MapLoader] File not found: {path}");
                    return null;
                }

                string json = File.ReadAllText(path);
                
                // EXTENSIVE DEBUG LOG: Check JSON for regions
                bool jsonHasRegions = json.Contains("\"regions\"", StringComparison.OrdinalIgnoreCase) || 
                                      json.Contains("\"Regions\"", StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"[MapLoader] === Loading map from {path} ===");
                Console.WriteLine($"[MapLoader] JSON contains 'regions' key: {jsonHasRegions}");
                
                // Try to parse regions from JSON manually for debugging
                try
                {
                    using (var doc = JsonDocument.Parse(json))
                    {
                        if (doc.RootElement.TryGetProperty("regions", out var regionsProp) || 
                            doc.RootElement.TryGetProperty("Regions", out regionsProp))
                        {
                            Console.WriteLine($"[MapLoader] Raw regions in JSON: count={regionsProp.GetArrayLength()}");
                            int idx = 0;
                            foreach (var regionElem in regionsProp.EnumerateArray())
                            {
                                if (idx < 3) // Log first 3 for debugging
                                {
                                    string rid = regionElem.TryGetProperty("id", out var idProp) ? idProp.GetString() : "?";
                                    int rtype = regionElem.TryGetProperty("type", out var typeProp) ? typeProp.GetInt32() : -1;
                                    if (regionElem.TryGetProperty("area", out var areaProp))
                                    {
                                        int l = areaProp.TryGetProperty("left", out var lProp) ? lProp.GetInt32() : 
                                                areaProp.TryGetProperty("Left", out var lProp2) ? lProp2.GetInt32() : 0;
                                        int r = areaProp.TryGetProperty("right", out var rProp) ? rProp.GetInt32() : 
                                                areaProp.TryGetProperty("Right", out var rProp2) ? rProp2.GetInt32() : 0;
                                        int t = areaProp.TryGetProperty("top", out var tProp) ? tProp.GetInt32() : 
                                                areaProp.TryGetProperty("Top", out var tProp2) ? tProp2.GetInt32() : 0;
                                        int b = areaProp.TryGetProperty("bottom", out var bProp) ? bProp.GetInt32() : 
                                                areaProp.TryGetProperty("Bottom", out var bProp2) ? bProp2.GetInt32() : 0;
                                        Console.WriteLine($"[MapLoader]   JSON region[{idx}]: id={rid} type={rtype} area L={l} R={r} T={t} B={b}");
                                    }
                                }
                                idx++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("[MapLoader]   WARNING: No 'regions' or 'Regions' property found in JSON!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MapLoader]   Error parsing JSON for debug: {ex.Message}");
                }
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                options.Converters.Add(new RectangleJsonConverter());

                var map = JsonSerializer.Deserialize<MapDefinition>(json, options);

                if (map == null)
                {
                    Console.WriteLine($"[MapLoader] Failed to deserialize: {path}");
                    return null;
                }
                
                // EXTENSIVE DEBUG LOG: After deserialization
                Console.WriteLine($"[MapLoader] Parsed regions: count={map.Regions?.Count ?? 0}");
                
                // Clean up invalid regions (from old saves with zeroed areas)
                if (map.Regions != null)
                {
                    int beforeCount = map.Regions.Count;
                    map.Regions = map.Regions
                        .Where(r => r.Area.Width > 0 && r.Area.Height > 0)
                        .ToList();
                    int removed = beforeCount - map.Regions.Count;
                    if (removed > 0)
                    {
                        Console.WriteLine($"[MapLoader]   Removed {removed} invalid regions (zero width/height)");
                    }
                }
                
                if (map.Regions != null && map.Regions.Count > 0)
                {
                    foreach (var region in map.Regions)
                    {
                        Console.WriteLine($"[MapLoader]   - id={region.Id} type={region.Type}({(int)region.Type}) areaTiles X={region.Area.X} Y={region.Area.Y} W={region.Area.Width} H={region.Area.Height}");
                        Console.WriteLine($"[MapLoader]     areaTiles L={region.Area.Left} R={region.Area.Right} T={region.Area.Top} B={region.Area.Bottom}");
                        
                        // Validate area sanity
                        if (region.Area.Width <= 0 || region.Area.Height <= 0)
                        {
                            Console.WriteLine($"[MapLoader]     WARNING: Invalid area size! W={region.Area.Width} H={region.Area.Height}");
                        }
                        if (region.Area.Left < 0 || region.Area.Top < 0 || 
                            region.Area.Right > map.MapWidth || region.Area.Bottom > map.MapHeight)
                        {
                            Console.WriteLine($"[MapLoader]     WARNING: Area out of bounds! Map={map.MapWidth}x{map.MapHeight}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[MapLoader]   WARNING: Regions list is null or empty after deserialization!");
                }
                
                Console.WriteLine($"[MapLoader] tileSize={map.TileSize} chunkSize={map.ChunkSize} mapSize={map.MapWidth}x{map.MapHeight}");

                // Backward compatibility: ensure ItemLayers exist
                if (map.ItemLayers == null || map.ItemLayers.Count == 0)
                {
                    Console.WriteLine("[MapLoader] Adding default ItemLayers for backward compatibility");
                    map.ItemLayers = new System.Collections.Generic.List<ItemLayerDefinition>
                    {
                        new ItemLayerDefinition { Name = "ItemsLow" },
                        new ItemLayerDefinition { Name = "ItemsHigh" }
                    };
                }
                else if (map.ItemLayers.Count < 2)
                {
                    // Ensure we have exactly 2 item layers
                    while (map.ItemLayers.Count < 2)
                    {
                        map.ItemLayers.Add(new ItemLayerDefinition 
                        { 
                            Name = map.ItemLayers.Count == 0 ? "ItemsLow" : "ItemsHigh" 
                        });
                    }
                }

                Console.WriteLine($"[MapLoader] Loaded map: {map.MapWidth}x{map.MapHeight} tiles, " +
                                  $"{map.TileLayers.Count} tile layers, " +
                                  $"{map.ItemLayers.Count} item layers, " +
                                  $"{map.BlockLayers.Count} block layers, " +
                                  $"{map.Regions.Count} regions");

                return map;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapLoader] Error loading map from {path}: {ex.Message}");
                Console.WriteLine($"[MapLoader] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Creates a default empty map with specified dimensions.
        /// If tileSizePx is 0 or negative, uses player size (32px) as default.
        /// </summary>
        public static MapDefinition CreateDefaultMap(int widthTiles, int heightTiles, int tileSizePx = 0, int chunkSizeTiles = 64)
        {
            // Use player size (32px) as default if not specified
            if (tileSizePx <= 0)
            {
                tileSizePx = 32; // Player sprite size
            }
            
            var map = new MapDefinition
            {
                MapWidth = widthTiles,
                MapHeight = heightTiles,
                TileSize = tileSizePx,
                ChunkSize = chunkSizeTiles
            };
            
            // Add default ground layer
            map.TileLayers.Add(new TileLayerDefinition
            {
                Name = "Ground",
                IsCollisionLayer = false
            });
            
            // Add default item layers (exactly 2)
            map.ItemLayers.Add(new ItemLayerDefinition
            {
                Name = "ItemsLow"
            });
            map.ItemLayers.Add(new ItemLayerDefinition
            {
                Name = "ItemsHigh"
            });
            
            // Add default block layer
            map.BlockLayers.Add(new BlockLayerDefinition
            {
                Name = "Blocks",
                IsCollisionLayer = true
            });
            
            // Add a default PlayerSpawn region in the center (TILE COORDINATES)
            int centerTileX = widthTiles / 2;
            int centerTileY = heightTiles / 2;
            int spawnSizeTiles = 10; // 10x10 tiles spawn area
            map.Regions.Add(new RegionDefinition
            {
                Id = "player_spawn_1",
                Type = RegionType.PlayerSpawn,
                Area = new Rectangle(
                    centerTileX - spawnSizeTiles / 2,
                    centerTileY - spawnSizeTiles / 2,
                    spawnSizeTiles,
                    spawnSizeTiles
                ), // TILE COORDINATES
                Meta = new System.Collections.Generic.Dictionary<string, string>()
            });
            
            // Add a large EnemySpawn region covering most of map (TILE COORDINATES)
            map.Regions.Add(new RegionDefinition
            {
                Id = "enemy_spawn_1",
                Type = RegionType.EnemySpawn,
                Area = new Rectangle(0, 0, widthTiles, heightTiles), // TILE COORDINATES
                Meta = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["maxEnemies"] = "100",
                    ["intervalSeconds"] = "2"
                }
            });
            
            Console.WriteLine($"[MapLoader] Created default map: {widthTiles}x{heightTiles} tiles");
            return map;
        }
        
        /// <summary>
        /// Attempts to convert an old world1.json format to MapDefinition.
        /// Provides backward compatibility.
        /// </summary>
        /// <param name="oldPath">Path to the old world1.json file.</param>
        /// <returns>A new MapDefinition with converted data, or null if conversion fails.</returns>
        public static MapDefinition ConvertFromLegacyWorld(string oldPath)
        {
            try
            {
                if (!File.Exists(oldPath))
                {
                    Console.WriteLine($"[MapLoader] Legacy file not found: {oldPath}");
                    return null;
                }

                string json = File.ReadAllText(oldPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var legacy = JsonSerializer.Deserialize<JsonWorldDefinition>(json, options);
                if (legacy == null)
                {
                    Console.WriteLine($"[MapLoader] Failed to deserialize legacy world: {oldPath}");
                    return null;
                }

                // Create new V2 map
                var mapV2 = new MapDefinition
                {
                    MapWidth = legacy.MapWidth / 128, // Convert pixels to tiles
                    MapHeight = legacy.MapHeight / 128,
                    ChunkSize = 64,
                    TileSize = 128
                };

                // Add default ground layer
                mapV2.TileLayers.Add(new TileLayerDefinition
                {
                    Name = "Ground",
                    IsCollisionLayer = false
                });

                // Add default block layer
                mapV2.BlockLayers.Add(new BlockLayerDefinition
                {
                    Name = "Blocks",
                    IsCollisionLayer = true
                });

                // Convert safe zones to regions
                foreach (var safeZone in legacy.SafeZones)
                {
                    mapV2.Regions.Add(new RegionDefinition
                    {
                        Id = $"safezone_{mapV2.Regions.Count}",
                        Type = RegionType.SafeZone,
                        Area = new Rectangle(safeZone.X, safeZone.Y, safeZone.Width, safeZone.Height),
                        Meta = new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["hasOpening"] = "true",
                            ["openingX"] = safeZone.Opening?.X.ToString() ?? "0",
                            ["openingY"] = safeZone.Opening?.Y.ToString() ?? "0",
                            ["openingWidth"] = safeZone.Opening?.Width.ToString() ?? "50",
                            ["openingHeight"] = safeZone.Opening?.Height.ToString() ?? "50"
                        }
                    });
                }

                // Convert wood spawn regions
                foreach (var woodRegion in legacy.WoodSpawnRegions)
                {
                    mapV2.Regions.Add(new RegionDefinition
                    {
                        Id = $"wood_{mapV2.Regions.Count}",
                        Type = RegionType.WoodSpawn,
                        Area = new Rectangle(woodRegion.X, woodRegion.Y, woodRegion.Width, woodRegion.Height),
                        Meta = new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["maxActive"] = woodRegion.MaxActiveWood.ToString(),
                            ["intervalSeconds"] = "5"
                        }
                    });
                }

                // Convert gold spawn regions
                foreach (var goldRegion in legacy.GoldSpawnRegions)
                {
                    mapV2.Regions.Add(new RegionDefinition
                    {
                        Id = $"gold_{mapV2.Regions.Count}",
                        Type = RegionType.GoldSpawn,
                        Area = new Rectangle(goldRegion.X, goldRegion.Y, goldRegion.Width, goldRegion.Height),
                        Meta = new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["maxActive"] = goldRegion.MaxActiveGold.ToString(),
                            ["intervalSeconds"] = "8"
                        }
                    });
                }

                // Convert biomes to regions
                foreach (var biome in legacy.Biomes)
                {
                    mapV2.Regions.Add(new RegionDefinition
                    {
                        Id = $"biome_{biome.Type}",
                        Type = RegionType.Biome,
                        Area = new Rectangle(biome.X, biome.Y, biome.Width, biome.Height),
                        Meta = new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["biomeType"] = biome.Type,
                            ["allowsEnemySpawns"] = biome.AllowsEnemySpawns.ToString(),
                            ["treeDensity"] = biome.TreeDensity.ToString(),
                            ["goldDensity"] = biome.GoldDensity.ToString(),
                            ["texture"] = biome.Texture
                        }
                    });
                }

                Console.WriteLine($"[MapLoader] Converted legacy world to V2: {mapV2.Regions.Count} regions");
                return mapV2;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapLoader] Error converting legacy world: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Loads an existing map or creates a default one if none exist.
        /// </summary>
        public static MapDefinition LoadOrCreateMap()
        {
            string mapsDir = Path.Combine("assets", "maps");
            
            // Ensure maps directory exists
            if (!Directory.Exists(mapsDir))
            {
                Directory.CreateDirectory(mapsDir);
                Console.WriteLine($"[MapLoader] Created maps directory: {mapsDir}");
            }
            
            // Find existing maps
            var mapFiles = Directory.GetFiles(mapsDir, "*.json");
            
            if (mapFiles.Length > 0)
            {
                // Load first available map
                string mapPath = mapFiles[0];
                Console.WriteLine($"[MapLoader] Found existing map: {mapPath}");
                var map = Load(mapPath);
                if (map != null) return map;
            }
            
            // No maps exist, create default
            Console.WriteLine("[MapLoader] No maps found, creating default map");
            var defaultMap = CreateDefaultMap(31, 31, 32, 64); // Use player size (32px)
            
            // Save it
            string defaultPath = Path.Combine(mapsDir, "world1.json");
            if (MapSaver.Save(defaultPath, defaultMap))
            {
                Console.WriteLine($"[MapLoader] Created and saved default map: {defaultPath}");
            }
            
            return defaultMap;
        }
        
        /// <summary>
        /// Prints a summary of the map definition to console.
        /// </summary>
        public static void PrintMapSummary(MapDefinition map)
        {
            if (map == null)
            {
                Console.WriteLine("[MapLoader] Map is null!");
                return;
            }
            
            Console.WriteLine("=== MAP SUMMARY ===");
            Console.WriteLine($"Size: {map.MapWidth}x{map.MapHeight} tiles");
            Console.WriteLine($"Tile Size: {map.TileSize}px");
            Console.WriteLine($"Chunk Size: {map.ChunkSize} tiles");
            Console.WriteLine($"Tile Layers: {map.TileLayers.Count}");
            Console.WriteLine($"Block Layers: {map.BlockLayers.Count}");
            Console.WriteLine($"Regions: {map.Regions.Count}");
            
            if (map.Regions.Count > 0)
            {
                Console.WriteLine("Region Types:");
                foreach (var region in map.Regions)
                {
                    Console.WriteLine($"  - {region.Type}: {region.Id}");
                }
            }
            
            Console.WriteLine("===================");
        }

        /// <summary>
        /// Gets a list of all available map file paths in assets/maps directory.
        /// </summary>
        /// <returns>List of absolute paths to .json map files, ordered by filename.</returns>
        public static System.Collections.Generic.List<string> GetAvailableMapPaths()
        {
            string mapsDir = Path.Combine("assets", "maps");
            
            if (!Directory.Exists(mapsDir))
            {
                Console.WriteLine($"[MapLoader] Maps directory does not exist: {mapsDir}, creating it...");
                Directory.CreateDirectory(mapsDir);
                return new System.Collections.Generic.List<string>();
            }

            var mapFiles = Directory.GetFiles(mapsDir, "*.json", SearchOption.TopDirectoryOnly);
            var result = new System.Collections.Generic.List<string>(mapFiles);
            result.Sort(); // Sort alphabetically
            
            Console.WriteLine($"[MapLoader] Found {result.Count} map(s) in {mapsDir}");
            return result;
        }

        /// <summary>
        /// Gets map info (name and dimensions) without fully loading the map.
        /// </summary>
        public static (string name, int width, int height) GetMapInfo(string path)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(path);
                
                // Quick parse to get dimensions only
                string json = File.ReadAllText(path);
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                int width = root.TryGetProperty("MapWidth", out var w) ? w.GetInt32() : 0;
                int height = root.TryGetProperty("MapHeight", out var h) ? h.GetInt32() : 0;
                
                return (name, width, height);
            }
            catch
            {
                return (Path.GetFileNameWithoutExtension(path), 0, 0);
            }
        }
    }
}

