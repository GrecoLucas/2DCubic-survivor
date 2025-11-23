using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Registry for discovering and managing map files.
    /// Scans assets/maps directory and provides map metadata.
    /// </summary>
    public static class MapRegistry
    {
        public class MapInfo
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public int WidthTiles { get; set; }
            public int HeightTiles { get; set; }
            public int TileSize { get; set; }
            public int ChunkSize { get; set; }
            public DateTime LastWriteTime { get; set; }
            public bool IsValid { get; set; }
        }

        /// <summary>
        /// Scans the maps directory and returns list of available maps.
        /// </summary>
        public static List<MapInfo> ScanMaps(string folder = null)
        {
            folder ??= Path.Combine("assets", "maps");

            Console.WriteLine($"[MapRegistry] Scanning: {folder}");

            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"[MapRegistry] Directory not found, creating: {folder}");
                Directory.CreateDirectory(folder);
                
                // Create a default map if none exist
                CreateDefaultMap(folder);
                
                // Rescan after creating default
                return ScanMaps(folder);
            }

            var mapFiles = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly)
                .Where(f => !Path.GetFileName(f).Contains("_autosave")) // Ignore autosaves
                .OrderBy(f => f)
                .ToList();

            Console.WriteLine($"[MapRegistry] Found {mapFiles.Count} map file(s)");

            var mapInfos = new List<MapInfo>();

            foreach (var filePath in mapFiles)
            {
                try
                {
                    var info = LoadInfo(filePath);
                    mapInfos.Add(info);
                    Console.WriteLine($"[MapRegistry]   ✓ {info.Name} ({info.WidthTiles}x{info.HeightTiles})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MapRegistry]   ✗ Failed to load {Path.GetFileName(filePath)}: {ex.Message}");
                    
                    // Add invalid entry so user knows it exists but is broken
                    mapInfos.Add(new MapInfo
                    {
                        Path = filePath,
                        Name = Path.GetFileNameWithoutExtension(filePath) + " (INVALID)",
                        IsValid = false,
                        LastWriteTime = File.GetLastWriteTime(filePath)
                    });
                }
            }

            return mapInfos;
        }

        /// <summary>
        /// Loads map metadata from JSON file without fully loading the map.
        /// </summary>
        public static MapInfo LoadInfo(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Map file not found: {path}");
            }

            var (name, width, height) = MapLoader.GetMapInfo(path);
            
            // Quick parse to get additional fields
            string json = File.ReadAllText(path);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Try both camelCase and PascalCase (MapSaver uses camelCase)
            int tileSize = 32; // Default to player size
            if (root.TryGetProperty("tileSize", out var ts1))
                tileSize = ts1.GetInt32();
            else if (root.TryGetProperty("TileSize", out var ts2))
                tileSize = ts2.GetInt32();
            
            int chunkSize = 64; // Default
            if (root.TryGetProperty("chunkSize", out var cs1))
                chunkSize = cs1.GetInt32();
            else if (root.TryGetProperty("ChunkSize", out var cs2))
                chunkSize = cs2.GetInt32();

            return new MapInfo
            {
                Path = path,
                Name = name,
                WidthTiles = width,
                HeightTiles = height,
                TileSize = tileSize,
                ChunkSize = chunkSize,
                LastWriteTime = File.GetLastWriteTime(path),
                IsValid = true
            };
        }

        /// <summary>
        /// Deletes a map file with confirmation logging.
        /// </summary>
        public static bool DeleteMap(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[MapRegistry] Cannot delete, file not found: {path}");
                    return false;
                }

                string name = Path.GetFileName(path);
                Console.WriteLine($"[MapRegistry] Deleting map: {name}");
                
                File.Delete(path);
                
                Console.WriteLine($"[MapRegistry] ✓ Deleted: {name}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapRegistry] ✗ Delete failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a default starter map.
        /// </summary>
        private static void CreateDefaultMap(string folder)
        {
            Console.WriteLine("[MapRegistry] Creating default starter map...");
            
            var mapDef = MapLoader.CreateDefaultMap(256, 256, 32, 64); // Use player size (32px)
            string path = Path.Combine(folder, "starter_map.json");
            
            MapSaver.Save(path, mapDef);
            
            Console.WriteLine($"[MapRegistry] ✓ Created: {path}");
        }

        /// <summary>
        /// Validates a map name for new map creation.
        /// </summary>
        public static (bool isValid, string error) ValidateMapName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Name cannot be empty");

            if (name.Length > 50)
                return (false, "Name too long (max 50 characters)");

            // Check for invalid filename characters
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (name.Any(c => invalidChars.Contains(c)))
                return (false, "Name contains invalid characters");

            return (true, null);
        }

        /// <summary>
        /// Checks if a map with this name already exists.
        /// </summary>
        public static bool MapExists(string name, string folder = null)
        {
            folder ??= Path.Combine("assets", "maps");
            string path = Path.Combine(folder, $"{name}.json");
            return File.Exists(path);
        }
    }
}

