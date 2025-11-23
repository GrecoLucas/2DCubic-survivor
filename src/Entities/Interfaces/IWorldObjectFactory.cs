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
        /// <param name="width">Largura da caixa (deve ser tileSize)</param>
        /// <param name="height">Altura da caixa (deve ser tileSize)</param>
        /// <param name="isDestructible">Se a caixa pode ser destruída</param>
        /// <param name="maxHealth">Vida máxima da caixa (se destrutível)</param>
        /// <returns>A entidade criada</returns>
        Entity CreateCrate(IGameWorld world, Vector2 position, float width = 32f, float height = 32f, bool isDestructible = false, float maxHealth = 50f);

        /// <summary>
        /// Cria um bloco de parede na posição especificada.
        /// </summary>
        /// <param name="world">O mundo do jogo</param>
        /// <param name="position">Posição central do bloco de parede</param>
        /// <param name="width">Largura do bloco</param>
        /// <param name="height">Altura do bloco</param>
        /// <returns>A entidade criada</returns>
        Entity CreateWall(IGameWorld world, Vector2 position, float width = 32f, float height = 32f);
        
        /// <summary>
        /// Cria um bloco de rocha na posição especificada.
        /// </summary>
        /// <param name="world">O mundo do jogo</param>
        /// <param name="position">Posição central do bloco de rocha</param>
        /// <param name="size">Tamanho do bloco</param>
        /// <returns>A entidade criada</returns>
        Entity CreateRock(IGameWorld world, Vector2 position, float size = 32f);
    }
}

