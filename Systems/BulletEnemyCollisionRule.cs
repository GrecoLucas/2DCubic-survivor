using System.Collections.Generic;
using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Regra de colisão entre projéteis e inimigos
    /// </summary>
    public sealed class BulletEnemyCollisionRule : ICollisionRule
    {
        private readonly List<Entity> _bulletsToRemove;

        public BulletEnemyCollisionRule(List<Entity> bulletsToRemove)
        {
            _bulletsToRemove = bulletsToRemove;
        }

        public bool Matches(Entity a, Entity b)
        {
            bool aIsBullet = a.GetComponent<BulletComponent>() != null;
            bool bIsBullet = b.GetComponent<BulletComponent>() != null;

            bool aIsEnemy = a.GetComponent<EnemyComponent>() != null;
            bool bIsEnemy = b.GetComponent<EnemyComponent>() != null;

            return (aIsBullet && bIsEnemy) || (bIsBullet && aIsEnemy);
        }

        public void Handle(Entity a, Entity b, float deltaTime, IGameWorld world)
        {
            var bulletEntity = a.GetComponent<BulletComponent>() != null ? a : b;
            var enemyEntity = a.GetComponent<EnemyComponent>() != null ? a : b;

            var bullet = bulletEntity.GetComponent<BulletComponent>();
            var health = enemyEntity.GetComponent<HealthComponent>();

            if (bullet == null || health == null)
                return;

            health.TakeDamage(bullet.Damage);
            _bulletsToRemove.Add(bulletEntity);
        }
    }
}

