using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de detecção e resposta a colisões (usando Strategy Pattern)
    /// </summary>
    public sealed class CollisionSystem : GameSystem
    {
        private readonly List<ICollisionRule> _collisionRules;
        private readonly List<Entity> _bulletsToRemove;

        public CollisionSystem()
        {
            _bulletsToRemove = new List<Entity>();
            _collisionRules = new List<ICollisionRule>
            {
                new BulletEnemyCollisionRule(_bulletsToRemove),
                new PlayerEnemyCollisionRule()
            };
        }

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
                ApplyCollisionRules(entityA, entityB, deltaTime);
            }
        }

        private void ApplyCollisionRules(Entity entityA, Entity entityB, float deltaTime)
        {
            foreach (var rule in _collisionRules)
            {
                if (rule.Matches(entityA, entityB))
                {
                    rule.Handle(entityA, entityB, deltaTime, World);
                    // Não fazemos break - múltiplas regras podem se aplicar
                }
            }
        }
    }
}
