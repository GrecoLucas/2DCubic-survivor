using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar inimigos
    /// </summary>
    public static class EnemyEntity
    {
        public static Entity Create(GameWorld world, Vector2 position)
        {
            var enemy = world.CreateEntity("Enemy");

            // Adicionar componentes
            enemy.AddComponent(new TransformComponent(position));
            enemy.AddComponent(new SpriteComponent(Color.Red, 40f, 40f)); // Quadrado vermelho 40x40
            enemy.AddComponent(new VelocityComponent(150f));
            enemy.AddComponent(new AIComponent(150f)); // Velocidade de perseguição
            enemy.AddComponent(new EnemyComponent(10f, 1f)); // 10 de dano, 1 segundo de cooldown
            enemy.AddComponent(new HealthComponent(50f)); // 50 de vida
            enemy.AddComponent(new ColliderComponent(40f, 40f, "Enemy"));

            return enemy;
        }
    }
}