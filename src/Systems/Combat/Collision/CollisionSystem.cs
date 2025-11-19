using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Core.Spatial;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de detecção e resposta a colisões (usando Strategy Pattern com Spatial Hashing).
    /// Usa broad-phase (spatial index) + narrow-phase (regras específicas) para eficiência.
    /// </summary>
    public sealed class CollisionSystem : GameSystem
    {
        private readonly ISpatialIndex _spatialIndex;
        private readonly List<ICollisionRule> _collisionRules;
        private readonly List<Entity> _bulletsToRemove;

        public CollisionSystem(ISpatialIndex spatialIndex)
        {
            _spatialIndex = spatialIndex ?? throw new ArgumentNullException(nameof(spatialIndex));
            _bulletsToRemove = new List<Entity>();
            _collisionRules = new List<ICollisionRule>
            {
                new ObstacleCollisionRule(), // Primeiro: prevenir atravessar paredes
                new BulletObstacleCollisionRule(_bulletsToRemove), // Balas colidem com obstáculos
                new BulletEnemyCollisionRule(_bulletsToRemove),
                new PlayerEnemyCollisionRule()
            };
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _bulletsToRemove.Clear();

            // 1. Coletar todas as entidades com ColliderComponent
            var colliderEntities = new List<Entity>(World.GetEntitiesWithComponent<ColliderComponent>());

            // 2. Construir lookup rápido: Entity -> index na lista
            var indicesByEntity = new Dictionary<Entity, int>(colliderEntities.Count);
            for (int i = 0; i < colliderEntities.Count; i++)
            {
                indicesByEntity[colliderEntities[i]] = i;
            }

            // 3. Reconstruir spatial index (broad-phase)
            _spatialIndex.Clear();

            var boundsByEntity = new Dictionary<Entity, Rectangle>(colliderEntities.Count);

            foreach (var entity in colliderEntities)
            {
                var collider = entity.GetComponent<ColliderComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (collider == null || transform == null || !collider.Enabled)
                    continue;

                var bounds = collider.GetBounds(transform.Position);
                boundsByEntity[entity] = bounds;
                _spatialIndex.Register(entity, bounds);
            }

            // 4. Broad-phase: para cada entidade, consultar apenas entidades próximas
            for (int i = 0; i < colliderEntities.Count; i++)
            {
                var entityA = colliderEntities[i];

                if (!boundsByEntity.TryGetValue(entityA, out var boundsA))
                    continue;

                foreach (var entityB in _spatialIndex.Query(boundsA))
                {
                    // Evitar auto-colisão e pares duplicados
                    if (ReferenceEquals(entityA, entityB))
                        continue;

                    if (!indicesByEntity.TryGetValue(entityB, out var indexB))
                        continue;

                    if (indexB <= i)
                        continue;

                    // Narrow-phase: verificar colisão real e aplicar regras
                    HandleCollisionPair(entityA, entityB, deltaTime);
                }
            }

            // 5. Remover projéteis marcados para remoção
            foreach (var bullet in _bulletsToRemove)
            {
                World.RemoveEntity(bullet);
            }
        }

        /// <summary>
        /// Verifica e trata uma possível colisão entre duas entidades (narrow-phase).
        /// </summary>
        private void HandleCollisionPair(Entity entityA, Entity entityB, float deltaTime)
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

            // Verificar interseção exata
            if (boundsA.Intersects(boundsB))
            {
                ApplyCollisionRules(entityA, entityB, deltaTime);
            }
        }

        /// <summary>
        /// Aplica todas as regras de colisão que correspondem ao par de entidades.
        /// </summary>
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
