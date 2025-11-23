using System.Collections.Generic;
using CubeSurvivor.Game.Map;

namespace CubeSurvivor.Game.Editor
{
    /// <summary>
    /// Central place for default metadata values for each region type.
    /// Used when creating new regions to ensure they have correct spawn parameters.
    /// </summary>
    public static class RegionDefaults
    {
        /// <summary>
        /// Gets default metadata dictionary for a given region type.
        /// </summary>
        public static Dictionary<string, string> GetDefaults(RegionType type)
        {
            return type switch
            {
                RegionType.PlayerSpawn => GetPlayerSpawnDefaults(),
                RegionType.EnemySpawn => GetEnemySpawnDefaults(),
                RegionType.GoldSpawn => GetGoldSpawnDefaults(),
                RegionType.WoodSpawn => GetWoodSpawnDefaults(),
                RegionType.AppleSpawn => GetAppleSpawnDefaults(),
                RegionType.TreeSpawn => GetTreeSpawnDefaults(),
                RegionType.ItemSpawn => GetItemSpawnDefaults(),
                RegionType.SafeZone => GetSafeZoneDefaults(),
                RegionType.Biome => GetBiomeDefaults(),
                _ => new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Gets a human-readable description of what this region type does.
        /// </summary>
        public static string GetDescription(RegionType type)
        {
            return type switch
            {
                RegionType.PlayerSpawn => "Where the player spawns at game start. Only one allowed.",
                RegionType.EnemySpawn => "Spawns enemies over time. Configurable spawn rate and max count.",
                RegionType.GoldSpawn => "Spawns gold pickups periodically. Can respawn.",
                RegionType.WoodSpawn => "Spawns wood pickups periodically. Can respawn.",
                RegionType.AppleSpawn => "Spawns apple consumables periodically. Can respawn.",
                RegionType.TreeSpawn => "Spawns tree blocks periodically. Can respawn.",
                RegionType.ItemSpawn => "Spawns any item type (hammer, gun, etc.) periodically.",
                RegionType.SafeZone => "Safe area where enemies don't spawn.",
                RegionType.Biome => "Defines biome type for visual/mechanical effects.",
                _ => "Unknown region type."
            };
        }

        /// <summary>
        /// Gets a summary of default meta values for display in UI.
        /// </summary>
        public static string GetMetaSummary(RegionType type)
        {
            var defaults = GetDefaults(type);
            if (defaults.Count == 0)
                return "No settings";

            var parts = new List<string>();
            foreach (var kvp in defaults)
            {
                parts.Add($"{kvp.Key}: {kvp.Value}");
            }
            return string.Join(", ", parts);
        }

        private static Dictionary<string, string> GetPlayerSpawnDefaults()
        {
            return new Dictionary<string, string>();
        }

        private static Dictionary<string, string> GetEnemySpawnDefaults()
        {
            return new Dictionary<string, string>
            {
                ["maxEnemies"] = "100",
                ["intervalSeconds"] = "2",
                ["minDistanceFromPlayer"] = "6",
                ["allowBosses"] = "false"
            };
        }

        private static Dictionary<string, string> GetGoldSpawnDefaults()
        {
            return new Dictionary<string, string>
            {
                ["maxActive"] = "10",
                ["intervalSeconds"] = "6",
                ["respawn"] = "true",
                ["amountMin"] = "1",
                ["amountMax"] = "1"
            };
        }

        private static Dictionary<string, string> GetWoodSpawnDefaults()
        {
            return new Dictionary<string, string>
            {
                ["maxActive"] = "80",
                ["intervalSeconds"] = "4",
                ["respawn"] = "true",
                ["amountMin"] = "1",
                ["amountMax"] = "3"
            };
        }

        private static Dictionary<string, string> GetAppleSpawnDefaults()
        {
            return new Dictionary<string, string>
            {
                ["maxActive"] = "20",
                ["intervalSeconds"] = "8",
                ["respawn"] = "true",
                ["healAmount"] = "10"
            };
        }

        private static Dictionary<string, string> GetTreeSpawnDefaults()
        {
            return new Dictionary<string, string>
            {
                ["maxActive"] = "40",
                ["intervalSeconds"] = "10",
                ["respawn"] = "true"
            };
        }

        private static Dictionary<string, string> GetItemSpawnDefaults()
        {
            return new Dictionary<string, string>
            {
                ["itemId"] = "hammer",
                ["maxActive"] = "5",
                ["intervalSeconds"] = "12",
                ["respawn"] = "true",
                ["amountMin"] = "1",
                ["amountMax"] = "1"
            };
        }

        private static Dictionary<string, string> GetSafeZoneDefaults()
        {
            return new Dictionary<string, string>();
        }

        private static Dictionary<string, string> GetBiomeDefaults()
        {
            return new Dictionary<string, string>
            {
                ["biomeType"] = "forest"
            };
        }
    }
}

