using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que aplica velocidade às posições das entidades
    /// </summary>
    public class MovementSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var entity in World.GetEntitiesWithComponent<VelocityComponent>())
            {
                var velocity = entity.GetComponent<VelocityComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (velocity == null || transform == null || !velocity.Enabled)
                    continue;

                // Aplicar velocidade à posição
                transform.Position += velocity.Velocity * deltaTime;
            }
        }
    }
}
