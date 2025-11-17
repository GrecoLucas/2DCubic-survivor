using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Marca uma entidade como inimigo
    /// </summary>
    public class EnemyComponent : Component
    {
        public float Damage { get; set; }
        public float AttackCooldown { get; set; }
        public float TimeSinceLastAttack { get; set; }

        public EnemyComponent(float damage = 10f, float attackCooldown = 1f)
        {
            Damage = damage;
            AttackCooldown = attackCooldown;
            TimeSinceLastAttack = attackCooldown; // Pode atacar imediatamente
        }
    }
}