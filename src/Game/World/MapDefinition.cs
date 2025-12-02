using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Adaptador que mapeia LevelDefinition para estrutura de área.
    /// Permite que AreaManager trabalhe com a estrutura existente do jogo.
    /// </summary>
    public sealed class MapDefinition
    {
        public string Name { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public int TileSize { get; set; } = 32;
        
        public List<CrateDefinition> Crates { get; set; } = new();
        public List<SafeZoneDefinition> SafeZones { get; set; } = new();
        public List<PickupDefinition> Pickups { get; set; } = new();
        public List<WoodSpawnRegionDefinition> WoodSpawnRegions { get; set; } = new();
        public List<BiomeDefinition> Biomes { get; set; } = new();
        
        /// <summary>
        /// Cria MapDefinition a partir de LevelDefinition existente
        /// </summary>
        public static MapDefinition FromLevelDefinition(LevelDefinition levelDef)
        {
            return new MapDefinition
            {
                Name = levelDef.Name,
                MapWidth = levelDef.MapWidth ?? GameConfig.MapWidth,
                MapHeight = levelDef.MapHeight ?? GameConfig.MapHeight,
                TileSize = 32,
                Crates = new List<CrateDefinition>(levelDef.Crates),
                SafeZones = new List<SafeZoneDefinition>(levelDef.SafeZones),
                Pickups = new List<PickupDefinition>(levelDef.Pickups),
                WoodSpawnRegions = new List<WoodSpawnRegionDefinition>(levelDef.WoodSpawnRegions),
                Biomes = new List<BiomeDefinition>(levelDef.Biomes)
            };
        }
        
        /// <summary>
        /// Converte MapDefinition para LevelDefinition
        /// </summary>
        public LevelDefinition ToLevelDefinition()
        {
            var levelDef = new LevelDefinition
            {
                MapWidth = MapWidth,
                MapHeight = MapHeight
            };
            
            levelDef.Crates.AddRange(Crates);
            levelDef.SafeZones.AddRange(SafeZones);
            levelDef.Pickups.AddRange(Pickups);
            levelDef.WoodSpawnRegions.AddRange(WoodSpawnRegions);
            levelDef.Biomes.AddRange(Biomes);
            
            return levelDef;
        }
    }
}
