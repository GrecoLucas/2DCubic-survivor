using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Regra de colisão entre jogador e inimigos
    /// </summary>
    public sealed class PlayerEnemyCollisionRule : ICollisionRule
    {
        public bool Matches(Entity a, Entity b)
        {
            bool aIsPlayer = a.GetComponent<PlayerInputComponent>() != null;
            bool bIsPlayer = b.GetComponent<PlayerInputComponent>() != null;

            bool aIsEnemy = a.GetComponent<EnemyComponent>() != null;
            bool bIsEnemy = b.GetComponent<EnemyComponent>() != null;

            return (aIsPlayer && bIsEnemy) || (bIsPlayer && aIsEnemy);
        }

        public void Handle(Entity a, Entity b, float deltaTime, IGameWorld world)
        {
            var playerEntity = a.GetComponent<PlayerInputComponent>() != null ? a : b;
            var enemyEntity = a.GetComponent<EnemyComponent>() != null ? a : b;

            var playerHealth = playerEntity.GetComponent<HealthComponent>();
            var enemyComp = enemyEntity.GetComponent<EnemyComponent>();

            if (playerHealth == null || enemyComp == null)
                return;

            enemyComp.TimeSinceLastAttack += deltaTime;

            if (enemyComp.TimeSinceLastAttack >= enemyComp.AttackCooldown)
            {
                playerHealth.TakeDamage(enemyComp.Damage);
                enemyComp.TimeSinceLastAttack = 0f;
            }
        }
    }
}

