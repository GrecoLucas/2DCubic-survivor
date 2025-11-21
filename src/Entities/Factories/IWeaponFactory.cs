using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities.Factories
{
    /// <summary>
    /// Interface para criação de armas
    /// Princípio: Dependency Inversion Principle (DIP)
    /// </summary>
    public interface IWeaponFactory
    {
        /// <summary>
        /// Cria uma entidade de arma no mundo
        /// </summary>
        /// <param name="world">Mundo do jogo</param>
        /// <param name="position">Posição inicial</param>
        /// <param name="weaponType">Tipo de arma</param>
        /// <returns>Entidade da arma criada</returns>
        Entity CreateWeapon(IGameWorld world, Vector2 position, string weaponType);
    }
}
