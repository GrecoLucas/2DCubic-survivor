using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente que permite a uma entidade construir estruturas.
    /// </summary>
    public sealed class BuilderComponent : Component
    {
        /// <summary>
        /// Distância máxima em que o jogador pode construir.
        /// </summary>
        public float BuildRange { get; }

        /// <summary>
        /// Posição do mundo onde o jogador solicitou construção.
        /// Null quando não há solicitação pendente.
        /// </summary>
        public Vector2? RequestedBuildPosition { get; set; }

        public BuilderComponent(float buildRange)
        {
            BuildRange = buildRange;
        }

        /// <summary>
        /// Limpa a solicitação de construção.
        /// </summary>
        public void ClearRequest()
        {
            RequestedBuildPosition = null;
        }
    }
}

