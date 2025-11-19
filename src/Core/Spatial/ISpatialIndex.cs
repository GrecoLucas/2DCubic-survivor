using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Core.Spatial
{
    /// <summary>
    /// Interface para índices espaciais que permitem consultas eficientes de entidades por região.
    /// Usado para broad-phase collision detection e outras queries espaciais.
    /// </summary>
    public interface ISpatialIndex
    {
        /// <summary>
        /// Limpa todos os dados do índice.
        /// Tipicamente chamado no início de cada frame antes de re-registrar entidades.
        /// </summary>
        void Clear();

        /// <summary>
        /// Registra uma entidade no índice espacial com seus bounds especificados.
        /// </summary>
        /// <param name="entity">A entidade a registrar</param>
        /// <param name="bounds">Os bounds da entidade em coordenadas do mundo</param>
        void Register(Entity entity, Rectangle bounds);

        /// <summary>
        /// Retorna todas as entidades que podem potencialmente colidir com o retângulo fornecido.
        /// </summary>
        /// <param name="bounds">A área de consulta</param>
        /// <returns>Enumerável de entidades potencialmente sobrepostas</returns>
        IEnumerable<Entity> Query(Rectangle bounds);
    }
}

