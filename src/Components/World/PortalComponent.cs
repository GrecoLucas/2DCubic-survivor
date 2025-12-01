using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente que marca uma entidade como portal de teleporte entre áreas.
    /// Princípio: SRP - Responsável apenas por dados do portal
    /// </summary>
    public sealed class PortalComponent : Component
    {
        /// <summary>
        /// ID da área de destino
        /// </summary>
        public string DestinationAreaId { get; }
        
        /// <summary>
        /// Direção do portal (Left, Right, Up, Down)
        /// </summary>
        public PortalDirection Direction { get; }
        
        /// <summary>
        /// Posição relativa no destino (0-1, onde spawnar o player)
        /// </summary>
        public Vector2 RelativeSpawnPosition { get; }
        
        /// <summary>
        /// Cooldown para evitar teleportes múltiplos
        /// </summary>
        public float Cooldown { get; set; }
        
        public PortalComponent(string destinationAreaId, PortalDirection direction, Vector2 relativeSpawnPosition)
        {
            DestinationAreaId = destinationAreaId;
            Direction = direction;
            RelativeSpawnPosition = relativeSpawnPosition;
            Cooldown = 0f;
        }
    }
    
    public enum PortalDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}