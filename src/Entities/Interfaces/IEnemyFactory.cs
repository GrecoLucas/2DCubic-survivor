using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Interface para factory de criação de inimigos
    /// </summary>
    public interface IEnemyFactory
    {
        Entity CreateEnemy(IGameWorld world, Vector2 position);
    }
}

