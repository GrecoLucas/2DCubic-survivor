using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar inimigos
    /// </summary>
    public sealed class EnemyFactory : IEnemyFactory
    {
        public Entity CreateEnemy(IGameWorld world, Vector2 position)
        {
            var enemy = world.CreateEntity("Enemy");

            // Adicionar componentes
            enemy.AddComponent(new TransformComponent(position));
            // Inimigos usam a camada Entities
            enemy.AddComponent(new SpriteComponent(Color.Red, 40f, 40f, RenderLayer.Entities)); // Quadrado vermelho 40x40
            enemy.AddComponent(new VelocityComponent(150f));
            enemy.AddComponent(new AIComponent(150f)); // Velocidade de perseguição
            enemy.AddComponent(new EnemyComponent(10f, 1f)); // 10 de dano, 1 segundo de cooldown
            enemy.AddComponent(new HealthComponent(50f)); // 50 de vida
            enemy.AddComponent(new ColliderComponent(40f, 40f, ColliderTag.Enemy));
            
            // Adicionar loot drop (10% de chance de dropar cérebro)
            var lootDrop = new LootDropComponent();
            lootDrop.AddLoot("brain", 0.1f); // 10% de chance
            enemy.AddComponent(lootDrop);

            return enemy;
        }
    }
}

