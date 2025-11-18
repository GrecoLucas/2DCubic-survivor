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
                    // Se for inimigo, conceder XP ao jogador antes de remover
                    if (entity.GetComponent<EnemyComponent>() != null)
                    {
                        // Buscar primeiro jogador disponível
                        Entity player = null;
                        foreach (var e in World.GetEntitiesWithComponent<PlayerInputComponent>())
                        {
                            player = e;
                            break;
                        }

                        if (player != null)
                        {
                            var xp = player.GetComponent<XpComponent>();
                            if (xp != null)
                            {
                                const float xpPerEnemy = 20f; // cada inimigo vale 20 XP
                                bool leveled = xp.AddXp(xpPerEnemy);
                                if (leveled)
                                {
                                    // Marcar que o jogador deve escolher um upgrade (se ainda não marcado)
                                    if (!player.HasComponent<UpgradeRequestComponent>())
                                    {
                                        player.AddComponent(new UpgradeRequestComponent());
                                    }
                                }
                            }
                        }
                    }

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