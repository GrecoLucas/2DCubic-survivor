using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CubeSurvivor.Components;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Carrega definição do mundo a partir de maps.txt
    /// </summary>
    public static class WorldMapLoader
    {
        /// <summary>
        /// Carrega o mundo a partir de maps.txt
        /// </summary>
        public static WorldMapDefinition Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[WorldMapLoader] File not found: {path}");
                    return CreateDefault();
                }

                var worldMap = new WorldMapDefinition();
                var lines = File.ReadAllLines(path);
                
                Console.WriteLine($"[WorldMapLoader] Loading world map from {path}");
                
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    
                    // Skip comments and empty lines
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                        continue;
                    
                    // Parse line manually
                    var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 1)
                        continue;
                    
                    var areaId = parts[0];
                    
                    var area = new AreaDefinition
                    {
                        Id = areaId,
                        Name = areaId, // Default to ID until JSON is loaded
                        WidthTiles = worldMap.DefaultAreaWidthTiles,
                        HeightTiles = worldMap.DefaultAreaHeightTiles,
                        ConfigPath = $"assets/areas/{areaId}.json"
                    };
                    
                    // Parse connections (R:A2, L:A1, etc.)
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var connection = parts[i];
                        var connParts = connection.Split(':');
                        
                        if (connParts.Length != 2)
                            continue;
                        
                        var directionStr = connParts[0].ToUpper();
                        var destinationId = connParts[1];
                        
                        PortalDirection direction = directionStr switch
                        {
                            "R" => PortalDirection.Right,
                            "L" => PortalDirection.Left,
                            "U" => PortalDirection.Up,
                            "D" => PortalDirection.Down,
                            _ => PortalDirection.Right
                        };
                        
                        area.Connections[direction] = destinationId;
                        Console.WriteLine($"[WorldMapLoader]   {areaId} -> {direction} -> {destinationId}");
                    }
                    
                    worldMap.Areas.Add(area);
                }
                
                Console.WriteLine($"[WorldMapLoader] Loaded {worldMap.Areas.Count} areas");
                return worldMap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldMapLoader] Error loading world map: {ex.Message}");
                return CreateDefault();
            }
        }
        
        private static WorldMapDefinition CreateDefault()
        {
            Console.WriteLine("[WorldMapLoader] Creating default world map");
            
            var worldMap = new WorldMapDefinition();
            
            // A1 -> A2 -> A3
            var a1 = new AreaDefinition
            {
                Id = "A1",
                WidthTiles = worldMap.DefaultAreaWidthTiles,
                HeightTiles = worldMap.DefaultAreaHeightTiles,
                ConfigPath = "assets/areas/A1.json"
            };
            a1.Connections[PortalDirection.Right] = "A2";
            
            var a2 = new AreaDefinition
            {
                Id = "A2",
                WidthTiles = worldMap.DefaultAreaWidthTiles,
                HeightTiles = worldMap.DefaultAreaHeightTiles,
                ConfigPath = "assets/areas/A2.json"
            };
            a2.Connections[PortalDirection.Left] = "A1";
            a2.Connections[PortalDirection.Right] = "A3";
            
            var a3 = new AreaDefinition
            {
                Id = "A3",
                WidthTiles = worldMap.DefaultAreaWidthTiles,
                HeightTiles = worldMap.DefaultAreaHeightTiles,
                ConfigPath = "assets/areas/A3.json"
            };
            a3.Connections[PortalDirection.Left] = "A2";
            
            worldMap.Areas.AddRange(new[] { a1, a2, a3 });
            
            return worldMap;
        }
    }
}