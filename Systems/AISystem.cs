using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de IA que faz inimigos perseguirem o jogador
    /// </summary>
    public class AISystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            // Encontrar o jogador
            Entity player = null;
            foreach (var entity in World.GetEntitiesWithComponent<InputComponent>())
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
                    velocity.Velocity = direction * ai.ChaseSpeed;
                }
            }
        }
    }
}