using System.Collections.Generic;
using CubeSurvivor.Game.Map;

namespace CubeSurvivor.Systems.Core
{
    /// <summary>
    /// Provides spawn regions for various spawn types.
    /// Decouples spawn systems from map implementation.
    /// </summary>
    public interface ISpawnRegionProvider
    {
        /// <summary>
        /// Gets all regions of a specific type.
        /// </summary>
        /// <param name="type">The type of region to retrieve.</param>
        /// <returns>Enumerable of region definitions.</returns>
        IEnumerable<RegionDefinition> GetRegions(RegionType type);

        /// <summary>
        /// Gets a specific region by its ID.
        /// </summary>
        /// <param name="id">The region ID.</param>
        /// <returns>The region definition, or null if not found.</returns>
        RegionDefinition GetRegionById(string id);

        /// <summary>
        /// Gets all regions (of any type).
        /// </summary>
        IEnumerable<RegionDefinition> GetAllRegions();
    }
}





