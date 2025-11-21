using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CubeSurvivor
{
    /// <summary>
    /// Define uma caixa ou obstáculo no mapa.
    /// </summary>
    public sealed class CrateDefinition
    {
        public Vector2 Position { get; set; }
        public bool IsDestructible { get; set; }
        public float MaxHealth { get; set; } = 50f;
    }

    /// <summary>
    /// Define uma zona segura (safe zone) onde inimigos não podem spawnar.
    /// Geralmente cercada por paredes com uma abertura (porta).
    /// </summary>
    public sealed class SafeZoneDefinition
    {
        /// <summary>
        /// Área retangular da zona segura em coordenadas do mundo.
        /// </summary>
        public Rectangle Area { get; set; }

        /// <summary>
        /// Área retangular onde a "porta" ou abertura deve existir.
        /// Nenhuma parede será colocada nesta área.
        /// </summary>
        public Rectangle OpeningArea { get; set; }

        // Futuro: flags ou metadata (ex: isPlayerSpawnZone, nome, etc.)
    }

    /// <summary>
    /// Define um pickup no mapa (wood, hammer, apple, etc.).
    /// </summary>
    public sealed class PickupDefinition
    {
        public Vector2 Position { get; set; }
        public string Type { get; set; }
        public float Amount { get; set; }
    }

    /// <summary>
    /// Define uma região onde madeira pode spawnar periodicamente.
    /// </summary>
    public sealed class WoodSpawnRegionDefinition
    {
        public Rectangle Area { get; set; }
        public int MaxActiveWood { get; set; }
    }

    /// <summary>
    /// Define uma região onde ouro pode spawnar periodicamente.
    /// </summary>
    public sealed class GoldSpawnRegionDefinition
    {
        public Rectangle Area { get; set; }
        public int MaxActiveGold { get; set; }
    }

    /// <summary>
    /// Define um nível/mapa completo com todos os seus elementos.
    /// </summary>
    public sealed class LevelDefinition
    {
        /// <summary>
        /// Largura do mapa em pixels. Pode sobrescrever GameConfig.MapWidth se definida.
        /// </summary>
        public int? MapWidth { get; set; }

        /// <summary>
        /// Altura do mapa em pixels. Pode sobrescrever GameConfig.MapHeight se definida.
        /// </summary>
        public int? MapHeight { get; set; }

        /// <summary>
        /// Lista de caixas/obstáculos no mapa.
        /// </summary>
        public List<CrateDefinition> Crates { get; } = new();

        /// <summary>
        /// Lista de zonas seguras (casas, etc.) no mapa.
        /// </summary>
        public List<SafeZoneDefinition> SafeZones { get; } = new();

        /// <summary>
        /// Lista de pickups colocados no mapa no início do jogo.
        /// </summary>
        public List<PickupDefinition> Pickups { get; } = new();

        /// <summary>
        /// Lista de regiões onde recursos (madeira) podem spawnar periodicamente.
        /// </summary>
        public List<WoodSpawnRegionDefinition> WoodSpawnRegions { get; } = new();

        /// <summary>
        /// Lista de regiões onde ouro pode spawnar periodicamente.
        /// </summary>
        public List<GoldSpawnRegionDefinition> GoldSpawnRegions { get; } = new();

        /// <summary>
        /// Lista de biomas definidos pelo nível.
        /// </summary>
        public List<BiomeDefinition> Biomes { get; } = new();
    }
}

    /// <summary>
    /// Representa um bioma carregado do JSON dentro do LevelDefinition.
    /// </summary>
    public sealed class BiomeDefinition
    {
        public Microsoft.Xna.Framework.Rectangle Area { get; set; }
        public CubeSurvivor.World.Biomes.BiomeType Type { get; set; } = CubeSurvivor.World.Biomes.BiomeType.Unknown;
        public bool AllowsEnemySpawns { get; set; } = true;
        public int TreeDensity { get; set; } = 0;
        public int GoldDensity { get; set; } = 0;
        public string TextureKey { get; set; }
    }

