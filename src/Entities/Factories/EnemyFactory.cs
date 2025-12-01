using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using System.Reflection;

namespace CubeSurvivor.Entities.Factories
{
    /// <summary>
    /// Factory para criar inimigos com base em definições JSON.
    /// </summary>
    public sealed class EnemyFactory : IEnemyFactory
    {
        private readonly Dictionary<string, EnemyData> _enemyTemplates = new();

        public EnemyFactory()
        {
            LoadEnemyTemplates();
        }

        private void LoadEnemyTemplates()
        {
            string path = Path.Combine("assets", "enemys");
            if (!Directory.Exists(path))
            {
                Console.WriteLine($"[EnemyFactory] Directory not found: {path}");
                return;
            }

            var files = Directory.GetFiles(path, "*.json");
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var enemyData = JsonSerializer.Deserialize<EnemyData>(json, options);

                    if (enemyData != null && !string.IsNullOrEmpty(enemyData.Id))
                    {
                        _enemyTemplates[enemyData.Id.ToLower()] = enemyData;
                        Console.WriteLine($"[EnemyFactory] Loaded enemy template: {enemyData.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EnemyFactory] Error loading {file}: {ex.Message}");
                }
            }
        }

        public Entity CreateEnemy(IGameWorld world, Vector2 position, string enemyId = "basic")
        {
            if (!_enemyTemplates.TryGetValue(enemyId.ToLower(), out var data))
            {
                Console.WriteLine($"[EnemyFactory] Enemy ID '{enemyId}' not found. Using default/fallback.");
                // Tentar fallback para 'basic' se existir, senão hardcode
                if (!_enemyTemplates.TryGetValue("basic", out data))
                {
                    // Fallback extremo se nem o basic.json existir
                    return CreateFallbackEnemy(world, position);
                }
            }

            var enemy = world.CreateEntity("Enemy");

            // Parse Color
            Color color = Color.Red;
            var colorProp = typeof(Color).GetProperty(data.Color, BindingFlags.Static | BindingFlags.Public);
            if (colorProp != null)
            {
                color = (Color)colorProp.GetValue(null);
            }
            else
            {
                // Tentar parse de string "R,G,B" ou "R,G,B,A" se necessário, mas por enquanto assume nomes
            }

            // Adicionar componentes
            enemy.AddComponent(new TransformComponent(position));
            enemy.AddComponent(new SpriteComponent(color, data.Width, data.Height, RenderLayer.Entities));
            enemy.AddComponent(new VelocityComponent(data.Speed));
            enemy.AddComponent(new AIComponent(data.Speed));
            enemy.AddComponent(new EnemyComponent(data.Damage, 1f));
            enemy.AddComponent(new HealthComponent(data.Health));
            enemy.AddComponent(new ColliderComponent(data.Width, data.Height, ColliderTag.Enemy));

            // Loot
            if (data.LootTable != null && data.LootTable.Count > 0)
            {
                var lootDrop = new LootDropComponent();
                foreach (var item in data.LootTable)
                {
                    lootDrop.AddLoot(item.Item, item.Chance);
                }
                enemy.AddComponent(lootDrop);
            }

            return enemy;
        }

        private Entity CreateFallbackEnemy(IGameWorld world, Vector2 position)
        {
            var enemy = world.CreateEntity("Enemy");
            enemy.AddComponent(new TransformComponent(position));
            enemy.AddComponent(new SpriteComponent(Color.Red, 40f, 40f, RenderLayer.Entities));
            enemy.AddComponent(new VelocityComponent(150f));
            enemy.AddComponent(new AIComponent(150f));
            enemy.AddComponent(new EnemyComponent(10f, 1f));
            enemy.AddComponent(new HealthComponent(50f));
            enemy.AddComponent(new ColliderComponent(40f, 40f, ColliderTag.Enemy));
            return enemy;
        }
    }
}
