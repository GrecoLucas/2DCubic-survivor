using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que gerencia projéteis (movimento, lifetime, remoção)
    /// </summary>
    public sealed class BulletSystem : GameSystem
    {
        private const int MapWidth = 2000;
        private const int MapHeight = 2000;

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var bulletsToRemove = new List<Entity>();

            foreach (var entity in World.GetEntitiesWithComponent<BulletComponent>())
            {
                // Pular se a entidade não está ativa (já foi marcada para remoção)
                if (!entity.Active)
                {
                    bulletsToRemove.Add(entity);
                    continue;
                }

                var bullet = entity.GetComponent<BulletComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (bullet == null || transform == null || !bullet.Enabled)
                    continue;

                // Atualizar lifetime
                bullet.Lifetime += deltaTime;

                // Remover se expirou ou saiu do mapa
                if (bullet.Lifetime >= bullet.MaxLifetime ||
                    transform.Position.X < 0 || transform.Position.X > MapWidth ||
                    transform.Position.Y < 0 || transform.Position.Y > MapHeight)
                {
                    bulletsToRemove.Add(entity);
                }
            }

            // Remover projéteis que devem ser removidos
            foreach (var bullet in bulletsToRemove)
            {
                World.RemoveEntity(bullet);
            }
        }
    }
}

