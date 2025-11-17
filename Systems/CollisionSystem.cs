using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de detecção e resposta a colisões
    /// </summary>
    public class CollisionSystem : GameSystem
    {
        private readonly List<Entity> _bulletsToRemove = new List<Entity>();

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _bulletsToRemove.Clear();

            // Coletar todas as entidades com colliders
            var entities = new List<Entity>(World.GetEntitiesWithComponent<ColliderComponent>());

            // Verificar colisões entre todas as entidades
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = i + 1; j < entities.Count; j++)
                {
                    CheckCollision(entities[i], entities[j], deltaTime);
                }
            }

            // Remover projéteis que colidiram
            foreach (var bullet in _bulletsToRemove)
            {
                World.RemoveEntity(bullet);
            }
        }

        private void CheckCollision(Entity entityA, Entity entityB, float deltaTime)
        {
            var colliderA = entityA.GetComponent<ColliderComponent>();
            var colliderB = entityB.GetComponent<ColliderComponent>();
            var transformA = entityA.GetComponent<TransformComponent>();
            var transformB = entityB.GetComponent<TransformComponent>();

            if (colliderA == null || colliderB == null || transformA == null || transformB == null)
                return;

            if (!colliderA.Enabled || !colliderB.Enabled)
                return;

            // Obter bounds
            var boundsA = colliderA.GetBounds(transformA.Position);
            var boundsB = colliderB.GetBounds(transformB.Position);

            // Verificar interseção
            if (boundsA.Intersects(boundsB))
            {
                HandleCollision(entityA, entityB, deltaTime);
            }
        }

        private void HandleCollision(Entity entityA, Entity entityB, float deltaTime)
        {
            // Colisão: Projétil vs Inimigo
            var bulletA = entityA.GetComponent<BulletComponent>();
            var enemyB = entityB.GetComponent<EnemyComponent>();

            if (bulletA != null && enemyB != null)
            {
                DamageEnemy(entityB, bulletA);
                // Marcar projétil para remoção
                _bulletsToRemove.Add(entityA);
                return;
            }

            var bulletB = entityB.GetComponent<BulletComponent>();
            var enemyA = entityA.GetComponent<EnemyComponent>();

            if (bulletB != null && enemyA != null)
            {
                DamageEnemy(entityA, bulletB);
                // Marcar projétil para remoção
                _bulletsToRemove.Add(entityB);
                return;
            }

            // Colisão: Jogador vs Inimigo
            var playerA = entityA.GetComponent<InputComponent>();
            if (playerA != null && enemyB != null)
            {
                DamagePlayer(entityA, entityB, deltaTime);
                return;
            }

            var playerB = entityB.GetComponent<InputComponent>();
            if (playerB != null && enemyA != null)
            {
                DamagePlayer(entityB, entityA, deltaTime);
            }
        }

        private void DamageEnemy(Entity enemy, BulletComponent bullet)
        {
            var health = enemy.GetComponent<HealthComponent>();
            if (health == null || bullet == null)
                return;

            // Aplicar dano do projétil
            health.TakeDamage(bullet.Damage);
        }

        private void DamagePlayer(Entity player, Entity enemy, float deltaTime)
        {
            var health = player.GetComponent<HealthComponent>();
            var enemyComp = enemy.GetComponent<EnemyComponent>();

            if (health == null || enemyComp == null)
                return;

            // Aplicar dano com cooldown
            enemyComp.TimeSinceLastAttack += deltaTime;

            if (enemyComp.TimeSinceLastAttack >= enemyComp.AttackCooldown)
            {
                health.TakeDamage(enemyComp.Damage);
                enemyComp.TimeSinceLastAttack = 0f;
            }
        }
    }
}