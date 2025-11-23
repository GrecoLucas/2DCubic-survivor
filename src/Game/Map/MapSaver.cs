using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CubeSurvivor.Game.Map.Serialization;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Saves MapDefinition to JSON files.
    /// Handles serialization with custom formatting.
    /// </summary>
    public static class MapSaver
    {
        /// <summary>
        /// Saves a MapDefinition to a JSON file.
        /// </summary>
        /// <param name="path">Path where the JSON file should be saved.</param>
        /// <param name="map">The map definition to save.</param>
        /// <returns>True if save was successful, false otherwise.</returns>
        public static bool Save(string path, MapDefinition map)
        {
            try
            {
                if (map == null)
                {
                    Console.WriteLine("[MapSaver] Cannot save null map");
                    return false;
                }

                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // EXTENSIVE DEBUG LOG: Before saving
                Console.WriteLine($"[MapSaver] === Saving map to {path} ===");
                Console.WriteLine($"[MapSaver] Regions count={map.Regions?.Count ?? 0}");
                
                if (map.Regions != null && map.Regions.Count > 0)
                {
                    foreach (var region in map.Regions)
                    {
                        Console.WriteLine($"[MapSaver]   - id={region.Id} type={(int)region.Type}({region.Type}) areaTiles L={region.Area.Left} R={region.Area.Right} T={region.Area.Top} B={region.Area.Bottom}");
                        if (region.Meta != null && region.Meta.Count > 0)
                        {
                            var metaStr = string.Join(", ", region.Meta.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                            Console.WriteLine($"[MapSaver]     meta: {metaStr}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[MapSaver]   WARNING: Regions list is null or empty!");
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new RectangleJsonConverter());

                string json = JsonSerializer.Serialize(map, options);
                
                // Check if JSON contains regions key
                bool hasRegionsKey = json.Contains("\"regions\"", StringComparison.OrdinalIgnoreCase) || 
                                     json.Contains("\"Regions\"", StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"[MapSaver] JSON contains key 'regions': {hasRegionsKey}");
                
                File.WriteAllText(path, json);
                
                long fileSize = new FileInfo(path).Length;
                Console.WriteLine($"[MapSaver] Saved bytes={fileSize}");
                Console.WriteLine($"[MapSaver] Successfully saved map to: {path}");
                Console.WriteLine($"[MapSaver] Map size: {map.MapWidth}x{map.MapHeight} tiles, " +
                                  $"tileSize={map.TileSize}px, " +
                                  $"{map.TileLayers.Count} tile layers, " +
                                  $"{map.BlockLayers.Count} block layers, " +
                                  $"{map.Regions?.Count ?? 0} regions");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapSaver] Error saving map to {path}: {ex.Message}");
                Console.WriteLine($"[MapSaver] Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Generates a default file name for a new map based on timestamp.
        /// </summary>
        /// <param name="baseDirectory">Base directory where map should be saved (e.g., "assets/maps").</param>
        /// <returns>A unique file path for the new map.</returns>
        public static string GenerateMapFileName(string baseDirectory = "assets/maps")
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"custom_map_{timestamp}.json";
            return Path.Combine(baseDirectory, fileName);
        }
    }
}

