using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities.Factories
{
    /// <summary>
    /// Interface para criação de inimigos
    /// Princípio: Dependency Inversion Principle (DIP) - Dependência em abstrações
    /// Princípio: Interface Segregation Principle (ISP) - Interface específica e coesa
    /// </summary>
    public interface IEnemyFactory
    {
        /// <summary>
        /// Cria uma entidade inimiga no mundo
        /// </summary>
        /// <param name="world">Mundo do jogo</param>
        /// <param name="position">Posição inicial</param>
        /// <param name="enemyType">Tipo de inimigo a criar</param>
        /// <returns>Entidade do inimigo criada</returns>
        Entity CreateEnemy(IGameWorld world, Vector2 position, string enemyType = "default");
    }
}
