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
    /// Define um nível/mapa completo com todos os seus elementos.
    /// </summary>
    public sealed class LevelDefinition
    {
        /// <summary>
        /// Lista de caixas/obstáculos no mapa.
        /// </summary>
        public List<CrateDefinition> Crates { get; } = new();

        /// <summary>
        /// Lista de zonas seguras (casas, etc.) no mapa.
        /// </summary>
        public List<SafeZoneDefinition> SafeZones { get; } = new();
    }
}

