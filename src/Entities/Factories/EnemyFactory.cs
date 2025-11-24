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
                // Check for animation frames (e.g., "glorb1", "glorb2", "glorb3")
                var baseName = definition.TextureName;
                if (baseName.EndsWith("1") && baseName.Length > 1)
                {
                    // Try to load animation frames
                    baseName = baseName.Substring(0, baseName.Length - 1);
                    var frame1 = _textureManager.GetTexture(baseName + "1");
                    var frame2 = _textureManager.GetTexture(baseName + "2");
                    var frame3 = _textureManager.GetTexture(baseName + "3");
                    
                    if (frame1 != null && frame2 != null && frame3 != null)
                    {
                        // Create animated sprite
                        var frames = new[] { frame1, frame2, frame3 };
                        enemy.AddComponent(new SpriteComponent(frame1, definition.Width, definition.Height, null, RenderLayer.Entities));
                        enemy.AddComponent(new SpriteAnimatorComponent(frames, fps: 10f));
                        Console.WriteLine($"[EnemyFactory] Spawned \"{enemyType}\" at ({position.X:F1},{position.Y:F1}) frames=3 idle={baseName}1");
                    }
                    else
                    {
                        Console.WriteLine($"[EnemyFactory] ⚠ Animation frames missing for '{enemyType}': frame1={frame1 != null}, frame2={frame2 != null}, frame3={frame3 != null}");
                        // Fallback to single texture
                        var texture = _textureManager.GetTexture(definition.TextureName);
                        if (texture != null)
                        {
                            enemy.AddComponent(new SpriteComponent(texture, definition.Width, definition.Height, null, RenderLayer.Entities));
                            Console.WriteLine($"[EnemyFactory] Spawned \"{enemyType}\" at ({position.X:F1},{position.Y:F1}) singleTexture={definition.TextureName}");
                        }
                        else
                        {
                            enemy.AddComponent(new SpriteComponent(color, definition.Width, definition.Height, RenderLayer.Entities));
                            Console.WriteLine($"[EnemyFactory] ⚠ Spawned \"{enemyType}\" at ({position.X:F1},{position.Y:F1}) with COLOR FALLBACK (texture '{definition.TextureName}' not found)");
                        }
                    }
                }
                else
                {
                    // Single texture
                    var texture = _textureManager.GetTexture(definition.TextureName);
                    if (texture != null)
                    {
                        enemy.AddComponent(new SpriteComponent(texture, definition.Width, definition.Height, null, RenderLayer.Entities));
                        Console.WriteLine($"[EnemyFactory] Spawned \"{enemyType}\" at ({position.X:F1},{position.Y:F1}) singleTexture={definition.TextureName}");
                    }
                    else
                    {
                        enemy.AddComponent(new SpriteComponent(color, definition.Width, definition.Height, RenderLayer.Entities));
                        Console.WriteLine($"[EnemyFactory] ⚠ Spawned \"{enemyType}\" at ({position.X:F1},{position.Y:F1}) with COLOR FALLBACK (texture '{definition.TextureName}' not found)");
                    }
                }
            }
            else
            {
                if (_textureManager == null)
                {
                    Console.WriteLine($"[EnemyFactory] ⚠ TextureManager is null for '{enemyType}', using color fallback");
                }
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

