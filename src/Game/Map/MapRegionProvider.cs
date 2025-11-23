using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Provides spawn regions from a MapDefinition.
    /// Implements the region provider interface for spawn systems.
    /// </summary>
    public sealed class MapRegionProvider : Systems.Core.ISpawnRegionProvider
    {
        private readonly ChunkedTileMap _map;

        public MapRegionProvider(ChunkedTileMap map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public IEnumerable<RegionDefinition> GetRegions(RegionType type)
        {
            return _map.Definition.Regions.Where(r => r.Type == type);
        }

        public RegionDefinition GetRegionById(string id)
        {
            return _map.Definition.Regions.FirstOrDefault(r => r.Id == id);
        }

        public IEnumerable<RegionDefinition> GetAllRegions()
        {
            return _map.Definition.Regions;
        }

        public int GetTileSize()
        {
            return _map.TileSize;
        }
    }
}

