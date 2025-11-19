using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Regra de colisão entre balas e obstáculos.
    /// Balas são destruídas ao colidir com obstáculos que bloqueiam projéteis.
    /// </summary>
    public sealed class BulletObstacleCollisionRule : ICollisionRule
    {
        private readonly List<Entity> _bulletsToRemove;

        public BulletObstacleCollisionRule(List<Entity> bulletsToRemove)
        {
            _bulletsToRemove = bulletsToRemove;
        }

        public bool Matches(Entity a, Entity b)
        {
            var aBullet = a.GetComponent<BulletComponent>();
            var bBullet = b.GetComponent<BulletComponent>();

            var aObstacle = a.GetComponent<ObstacleComponent>();
            var bObstacle = b.GetComponent<ObstacleComponent>();

            // Uma deve ser bala e a outra deve ser obstáculo que bloqueia balas
            bool aIsBullet = aBullet != null;
            bool bIsBullet = bBullet != null;
            bool aBlocksBullets = aObstacle != null && aObstacle.BlocksBullets;
            bool bBlocksBullets = bObstacle != null && bObstacle.BlocksBullets;

            return (aIsBullet && bBlocksBullets) || (bIsBullet && aBlocksBullets);
        }

        public void Handle(Entity a, Entity b, float deltaTime, IGameWorld world)
        {
            // Identificar qual é a bala e qual é o obstáculo
            var bullet = a.GetComponent<BulletComponent>() != null ? a : b;
            var obstacle = a.GetComponent<ObstacleComponent>() != null ? a : b;

            // Se o obstáculo for destrutível, aplicar dano
            var obstacleHealth = obstacle.GetComponent<HealthComponent>();
            if (obstacleHealth != null)
            {
                var bulletComp = bullet.GetComponent<BulletComponent>();
                if (bulletComp != null)
                {
                    obstacleHealth.TakeDamage(bulletComp.Damage);
                }
            }

            // Marcar bala para remoção
            if (!_bulletsToRemove.Contains(bullet))
            {
                _bulletsToRemove.Add(bullet);
            }
        }
    }
}

