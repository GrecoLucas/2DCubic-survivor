using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de IA que faz inimigos perseguirem o jogador
    /// </summary>
    public sealed class AISystem : GameSystem
    {
        private readonly CubeSurvivor.Systems.World.BiomeSystem _biomeSystem;

        public AISystem(CubeSurvivor.Systems.World.BiomeSystem biomeSystem = null)
        {
            _biomeSystem = biomeSystem;
        }

        public override void Update(GameTime gameTime)
        {
            // Encontrar o jogador
            Entity player = null;
            foreach (var entity in World.GetEntitiesWithComponent<PlayerInputComponent>())
            {
                player = entity;
                break;
            }

            if (player == null) return;

            var playerTransform = player.GetComponent<TransformComponent>();
            if (playerTransform == null) return;

            // Atualizar todos os inimigos
            foreach (var enemy in World.GetEntitiesWithComponent<AIComponent>())
            {
                var ai = enemy.GetComponent<AIComponent>();
                var transform = enemy.GetComponent<TransformComponent>();
                var velocity = enemy.GetComponent<VelocityComponent>();

                if (ai == null || transform == null || velocity == null || !ai.Enabled)
                    continue;

                // Calcular direção até o jogador
                Vector2 direction = playerTransform.Position - transform.Position;
                float distance = direction.Length();

                // Verificar se está no alcance de detecção
                if (distance > ai.DetectionRange)
                {
                    velocity.Velocity = Vector2.Zero;
                    continue;
                }

                // Normalizar e aplicar velocidade de perseguição
                if (distance > 0)
                {
                    direction.Normalize();

                    // Calcular deslocamento pretendido neste frame
                    var displacement = direction * ai.ChaseSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    var intendedPos = transform.Position + displacement;

                    // Se temos um BiomeSystem, impedir que inimigos saiam do bioma atual
                    if (_biomeSystem != null)
                    {
                        var currentBiome = _biomeSystem.GetBiomeAt(transform.Position);
                        var intendedBiome = _biomeSystem.GetBiomeAt(intendedPos);

                        // Se o inimigo está na caverna e o próximo passo o tiraria da caverna, pare-o
                        if (currentBiome != null && currentBiome.Type == CubeSurvivor.World.Biomes.BiomeType.Cave
                            && (intendedBiome == null || intendedBiome.Type != CubeSurvivor.World.Biomes.BiomeType.Cave))
                        {
                            velocity.Velocity = Vector2.Zero;
                            continue;
                        }
                    }

                    velocity.Velocity = direction * ai.ChaseSpeed;
                }
            }
        }
    }
}