using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que aplica velocidade às posições das entidades
    /// </summary>
    public sealed class MovementSystem : GameSystem
    {
        private readonly CubeSurvivor.Systems.World.BiomeSystem _biomeSystem;

        public MovementSystem(CubeSurvivor.Systems.World.BiomeSystem biomeSystem = null)
        {
            _biomeSystem = biomeSystem;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var entity in World.GetEntitiesWithComponent<VelocityComponent>())
            {
                var velocity = entity.GetComponent<VelocityComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (velocity == null || transform == null || !velocity.Enabled)
                    continue;

                // Calcular nova posição
                Vector2 newPos = transform.Position + velocity.Velocity * deltaTime;

                // Verificar se o bioma permite caminhar (apenas para o Player por enquanto, ou todos?)
                // Se for player, checar colisão com bioma
                if (_biomeSystem != null && entity.GetComponent<PlayerInputComponent>() != null)
                {
                    var biome = _biomeSystem.GetBiomeAt(newPos);
                    if (biome != null && !biome.IsWalkable)
                    {
                        // Bloquear movimento (colisão simples, para na borda)
                        // Para um sistema mais robusto, deslizar na parede seria ideal, mas stop é ok.
                        continue; 
                    }
                }

                // Aplicar velocidade à posição
                transform.Position = newPos;
            }
        }
    }
}
