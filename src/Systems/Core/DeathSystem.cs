using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que remove entidades mortas (sem vida)
    /// </summary>
    public sealed class DeathSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            var entitiesToRemove = new List<Entity>();

            foreach (var entity in World.GetEntitiesWithComponent<HealthComponent>())
            {
                var health = entity.GetComponent<HealthComponent>();
                if (health == null || !health.Enabled)
                    continue;

                // Se a entidade não está viva, marcar para remoção
                if (!health.IsAlive)
                {
                    entitiesToRemove.Add(entity);
                }
            }

            // Remover entidades mortas
            foreach (var entity in entitiesToRemove)
            {
                World.RemoveEntity(entity);
            }
        }
    }
}

