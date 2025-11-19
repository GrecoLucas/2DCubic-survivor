using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Regra de colisão entre entidades móveis e obstáculos.
    /// Impede que entidades passem através de paredes e obstáculos.
    /// </summary>
    public sealed class ObstacleCollisionRule : ICollisionRule
    {
        public bool Matches(Entity a, Entity b)
        {
            var aObstacle = a.GetComponent<ObstacleComponent>();
            var bObstacle = b.GetComponent<ObstacleComponent>();

            // Uma das entidades deve ser um obstáculo que bloqueia movimento
            var aBlocksMovement = aObstacle != null && aObstacle.BlocksMovement;
            var bBlocksMovement = bObstacle != null && bObstacle.BlocksMovement;

            if (!aBlocksMovement && !bBlocksMovement)
                return false;

            // A outra entidade deve ter velocidade (ser móvel)
            var aHasVelocity = a.GetComponent<VelocityComponent>() != null;
            var bHasVelocity = b.GetComponent<VelocityComponent>() != null;

            return (aBlocksMovement && bHasVelocity) || (bBlocksMovement && aHasVelocity);
        }

        public void Handle(Entity a, Entity b, float deltaTime, IGameWorld world)
        {
            // Identificar qual é o obstáculo e qual é a entidade móvel
            var obstacle = a.GetComponent<ObstacleComponent>() != null ? a : b;
            var movingEntity = a.GetComponent<VelocityComponent>() != null ? a : b;

            var obstacleTransform = obstacle.GetComponent<TransformComponent>();
            var movingTransform = movingEntity.GetComponent<TransformComponent>();
            var obstacleCollider = obstacle.GetComponent<ColliderComponent>();
            var movingCollider = movingEntity.GetComponent<ColliderComponent>();

            if (obstacleTransform == null || movingTransform == null || 
                obstacleCollider == null || movingCollider == null)
                return;

            // Calcular a separação necessária
            var separation = CalculateSeparation(
                movingTransform.Position,
                obstacleTransform.Position,
                movingCollider,
                obstacleCollider
            );

            // Empurrar a entidade móvel para fora do obstáculo
            movingTransform.Position += separation;
        }

        /// <summary>
        /// Calcula o vetor de separação necessário para resolver a colisão.
        /// </summary>
        private Vector2 CalculateSeparation(Vector2 movingPos, Vector2 obstaclePos, 
            ColliderComponent movingCollider, ColliderComponent obstacleCollider)
        {
            var movingBounds = movingCollider.GetBounds(movingPos);
            var obstacleBounds = obstacleCollider.GetBounds(obstaclePos);

            // Calcular a quantidade de sobreposição em cada eixo
            float overlapX = 0;
            float overlapY = 0;

            if (movingBounds.Center.X < obstacleBounds.Center.X)
            {
                // Entidade está à esquerda do obstáculo
                overlapX = movingBounds.Right - obstacleBounds.Left;
            }
            else
            {
                // Entidade está à direita do obstáculo
                overlapX = movingBounds.Left - obstacleBounds.Right;
            }

            if (movingBounds.Center.Y < obstacleBounds.Center.Y)
            {
                // Entidade está acima do obstáculo
                overlapY = movingBounds.Bottom - obstacleBounds.Top;
            }
            else
            {
                // Entidade está abaixo do obstáculo
                overlapY = movingBounds.Top - obstacleBounds.Bottom;
            }

            // Empurrar na direção com menor sobreposição (separação mínima)
            if (System.Math.Abs(overlapX) < System.Math.Abs(overlapY))
            {
                return new Vector2(-overlapX, 0);
            }
            else
            {
                return new Vector2(0, -overlapY);
            }
        }
    }
}

