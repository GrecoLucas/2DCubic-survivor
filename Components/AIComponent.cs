using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de IA para inimigos
    /// </summary>
    public class AIComponent : Component
    {
        public float ChaseSpeed { get; set; }
        public float DetectionRange { get; set; }
        public Entity Target { get; set; }

        public AIComponent(float chaseSpeed = 150f, float detectionRange = float.MaxValue)
        {
            ChaseSpeed = chaseSpeed;
            DetectionRange = detectionRange;
        }
    }
}