using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Interface para factory de criação de objetos do mundo (obstáculos, caixas, paredes, etc.)
    /// </summary>
    public interface IWorldObjectFactory
    {
        /// <summary>
        /// Cria uma caixa/obstáculo na posição especificada.
        /// </summary>
        /// <param name="world">O mundo do jogo</param>
        /// <param name="position">Posição central da caixa</param>
        /// <param name="isDestructible">Se a caixa pode ser destruída</param>
        /// <param name="maxHealth">Vida máxima da caixa (se destrutível)</param>
        /// <returns>A entidade criada</returns>
        Entity CreateCrate(IGameWorld world, Vector2 position, bool isDestructible = false, float maxHealth = 50f);

        /// <summary>
        /// Cria um bloco de parede na posição especificada.
        /// </summary>
        /// <param name="world">O mundo do jogo</param>
        /// <param name="position">Posição central do bloco de parede</param>
        /// <param name="width">Largura do bloco</param>
        /// <param name="height">Altura do bloco</param>
        /// <returns>A entidade criada</returns>
        Entity CreateWall(IGameWorld world, Vector2 position, float width = 50f, float height = 50f);
    }
}

