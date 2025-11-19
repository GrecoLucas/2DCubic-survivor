using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Interface para provedores de zonas de exclusão de spawn de inimigos.
    /// Zonas de exclusão são áreas retangulares onde inimigos não devem spawnar.
    /// </summary>
    public interface IEnemySpawnExclusionProvider
    {
        /// <summary>
        /// Retorna uma lista somente leitura de retângulos representando zonas de exclusão.
        /// </summary>
        IReadOnlyList<Rectangle> GetExclusionZones();
    }
}

