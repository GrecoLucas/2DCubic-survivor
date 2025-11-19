using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente que marca uma entidade como obstáculo que bloqueia movimento e/ou projéteis.
    /// </summary>
    public sealed class ObstacleComponent : Component
    {
        /// <summary>
        /// Se true, este obstáculo bloqueia movimento de entidades.
        /// </summary>
        public bool BlocksMovement { get; set; }

        /// <summary>
        /// Se true, este obstáculo bloqueia projéteis (balas).
        /// </summary>
        public bool BlocksBullets { get; set; }

        /// <summary>
        /// Se true, este obstáculo é destrutível (tem HealthComponent).
        /// </summary>
        public bool IsDestructible { get; set; }

        public ObstacleComponent(bool blocksMovement = true, bool blocksBullets = true, bool isDestructible = false)
        {
            BlocksMovement = blocksMovement;
            BlocksBullets = blocksBullets;
            IsDestructible = isDestructible;
        }
    }
}

