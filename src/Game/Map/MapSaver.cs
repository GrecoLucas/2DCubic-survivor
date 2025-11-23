using System;
using System.IO;
using System.Text.Json;

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

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(map, options);
                File.WriteAllText(path, json);

                Console.WriteLine($"[MapSaver] Successfully saved map to: {path}");
                Console.WriteLine($"[MapSaver] Map size: {map.MapWidth}x{map.MapHeight} tiles, " +
                                  $"{map.TileLayers.Count} tile layers, " +
                                  $"{map.BlockLayers.Count} block layers, " +
                                  $"{map.Regions.Count} regions");

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

