using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities.Factories
{
    /// <summary>
    /// Interface para criação de recursos coletáveis
    /// Princípio: Dependency Inversion Principle (DIP)
    /// </summary>
    public interface IResourceFactory
    {
        /// <summary>
        /// Cria uma entidade de recurso no mundo
        /// </summary>
        /// <param name="world">Mundo do jogo</param>
        /// <param name="position">Posição inicial</param>
        /// <param name="resourceType">Tipo de recurso (wood, gold, etc.)</param>
        /// <returns>Entidade do recurso criada</returns>
        Entity CreateResource(IGameWorld world, Vector2 position, string resourceType);
    }
}
