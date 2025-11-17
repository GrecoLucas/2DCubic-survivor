using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Marca uma entidade como controlada pelo jogador
    /// </summary>
    public class InputComponent : Component
    {
        public bool IsPlayerControlled { get; set; } = true;
        public float ShootCooldown { get; set; } = 0f;
        public float ShootCooldownTime { get; set; } = 0.2f; // 0.2 segundos entre tiros
    }
}
