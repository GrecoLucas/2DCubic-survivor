using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar objetos do mundo como obstáculos, caixas e paredes.
    /// </summary>
    public sealed class WorldObjectFactory : IWorldObjectFactory
    {
        public Entity CreateCrate(IGameWorld world, Vector2 position, bool isDestructible = false, float maxHealth = 50f)
        {
            var crate = world.CreateEntity("Crate");

            float crateSize = GameConfig.WallBlockSize;

            // Transformação e visual
            crate.AddComponent(new TransformComponent(position));
            crate.AddComponent(new SpriteComponent(new Color(139, 69, 19), crateSize, crateSize, RenderLayer.GroundEffects)); // Cor marrom

            // Componentes de física e colisão
            crate.AddComponent(new ColliderComponent(crateSize, crateSize, ColliderTag.Default));
            crate.AddComponent(new ObstacleComponent(
                blocksMovement: true,
                blocksBullets: true,
                isDestructible: isDestructible
            ));

            // Se destrutível, adicionar vida
            if (isDestructible)
            {
                crate.AddComponent(new HealthComponent(maxHealth));
            }

            return crate;
        }

        public Entity CreateWall(IGameWorld world, Vector2 position, float width = 50f, float height = 50f)
        {
            var wall = world.CreateEntity("Wall");

            // Transformação e visual
            wall.AddComponent(new TransformComponent(position));
            wall.AddComponent(new SpriteComponent(new Color(80, 80, 80), width, height, RenderLayer.GroundEffects)); // Cor cinza escuro

            // Componentes de física e colisão
            wall.AddComponent(new ColliderComponent(width, height, ColliderTag.Default));
            wall.AddComponent(new ObstacleComponent(
                blocksMovement: true,
                blocksBullets: true,
                isDestructible: false // Paredes são indestrutíveis
            ));

            return wall;
        }
    }
}

