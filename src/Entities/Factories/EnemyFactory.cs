using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities.Factories;
using CubeSurvivor.Game.Registries;
using Microsoft.Xna.Framework;
using System;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar inimigos baseada em definições do EnemyRegistry
    /// Princípio: Single Responsibility Principle (SRP) - Responsável apenas por criar inimigos
    /// Princípio: Open/Closed Principle (OCP) - Aberto para novos tipos via registry
    /// Princípio: Dependency Inversion Principle (DIP) - Depende de abstrações (IEnemyFactory)
    /// </summary>
    public sealed class EnemyFactory : IEnemyFactory
    {
        private readonly TextureManager _textureManager;

        public EnemyFactory(TextureManager textureManager = null)
        {
            _textureManager = textureManager;
        }

        public Entity CreateEnemy(IGameWorld world, Vector2 position, string enemyType = "default")
        {
            // Obter definição do registry
            if (!EnemyRegistry.Instance.Contains(enemyType))
            {
                throw new ArgumentException($"Enemy type '{enemyType}' not registered.");
            }

            var definition = EnemyRegistry.Instance.Get(enemyType);
            var enemy = world.CreateEntity(definition.Name);

            // Adicionar componentes baseados na definição
            enemy.AddComponent(new TransformComponent(position));

            // Sprite com cor ou textura
            var color = new Color(definition.ColorR, definition.ColorG, definition.ColorB);
            if (!string.IsNullOrEmpty(definition.TextureName) && _textureManager != null)
            {
                var texture = _textureManager.GetTexture(definition.TextureName);
                if (texture != null)
                {
                    enemy.AddComponent(new SpriteComponent(texture, definition.Width, definition.Height, null, RenderLayer.Entities));
                }
                else
                {
                    enemy.AddComponent(new SpriteComponent(color, definition.Width, definition.Height, RenderLayer.Entities));
                }
            }
            else
            {
                enemy.AddComponent(new SpriteComponent(color, definition.Width, definition.Height, RenderLayer.Entities));
            }

            enemy.AddComponent(new VelocityComponent(definition.Speed));
            enemy.AddComponent(new AIComponent(definition.Speed));
            enemy.AddComponent(new EnemyComponent(definition.Damage, definition.AttackCooldown));
            enemy.AddComponent(new HealthComponent(definition.Health));
            enemy.AddComponent(new ColliderComponent(definition.Width, definition.Height, ColliderTag.Enemy));

            // Adicionar loot drops
            if (definition.LootTable != null && definition.LootTable.Length > 0)
            {
                var lootDrop = new LootDropComponent();
                foreach (var loot in definition.LootTable)
                {
                    lootDrop.AddLoot(loot.ItemType, loot.DropChance);
                }
                enemy.AddComponent(lootDrop);
            }

            return enemy;
        }
    }
}

