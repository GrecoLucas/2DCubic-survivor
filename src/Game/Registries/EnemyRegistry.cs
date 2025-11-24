using System;
using CubeSurvivor.Core.Registry;

namespace CubeSurvivor.Game.Registries
{
    /// <summary>
    /// Registry para definições de inimigos
    /// Princípio: Single Responsibility Principle (SRP) - Gerencia apenas definições de inimigos
    /// Princípio: Open/Closed Principle (OCP) - Aberto para novos tipos de inimigos
    /// 
    /// Uso: Adicionar novos tipos de inimigos facilmente
    /// Exemplo:
    /// <code>
    /// var tankDefinition = new EnemyDefinition
    /// {
    ///     Name = "Tank",
    ///     Health = 200f,
    ///     Damage = 5f,
    ///     Speed = 75f,
    ///     AttackCooldown = 2f,
    ///     Width = 60f,
    ///     Height = 60f,
    ///     ColorR = 100,
    ///     ColorG = 100,
    ///     ColorB = 100,
    ///     LootTable = new[] { new LootEntry { ItemType = "brain", DropChance = 0.15f } }
    /// };
    /// EnemyRegistry.Instance.Register("tank", tankDefinition);
    /// </code>
    /// </summary>
    public sealed class EnemyRegistry : Registry<string, EnemyDefinition>
    {
        private static readonly Lazy<EnemyRegistry> _instance = 
            new Lazy<EnemyRegistry>(() => new EnemyRegistry());

        public static EnemyRegistry Instance => _instance.Value;

        private EnemyRegistry()
        {
            RegisterDefaultEnemies();
        }

        private void RegisterDefaultEnemies()
        {
            // Inimigo padrão (básico)
            Register("default", new EnemyDefinition
            {
                Name = "Default Enemy",
                Health = 50f,
                Damage = 10f,
                Speed = 150f,
                AttackCooldown = 1f,
                Width = 40f,
                Height = 40f,
                ColorR = 255,
                ColorG = 0,
                ColorB = 0,
                LootTable = new[]
                {
                    new LootEntry { ItemType = "brain", DropChance = 0.1f }
                }
            });

            // Exemplo de como adicionar um inimigo rápido
            Register("fast", new EnemyDefinition
            {
                Name = "Fast Enemy",
                Health = 30f,
                Damage = 8f,
                Speed = 250f,
                AttackCooldown = 0.8f,
                Width = 30f,
                Height = 30f,
                ColorR = 255,
                ColorG = 100,
                ColorB = 0,
                LootTable = new[]
                {
                    new LootEntry { ItemType = "brain", DropChance = 0.05f }
                }
            });

            // Exemplo de como adicionar um inimigo forte
            Register("strong", new EnemyDefinition
            {
                Name = "Strong Enemy",
                Health = 100f,
                Damage = 20f,
                Speed = 100f,
                AttackCooldown = 1.5f,
                Width = 50f,
                Height = 50f,
                ColorR = 150,
                ColorG = 0,
                ColorB = 0,
                LootTable = new[]
                {
                    new LootEntry { ItemType = "brain", DropChance = 0.25f }
                }
            });

            // Glorb enemy with animated sprite (glorb1, glorb2, glorb3 frames)
            Register("glorb", new EnemyDefinition
            {
                Name = "Glorb",
                Health = 50f,
                Damage = 10f,
                Speed = 150f,
                AttackCooldown = 1f,
                Width = 32f,
                Height = 32f,
                ColorR = 100,
                ColorG = 150,
                ColorB = 200,
                TextureName = "glorb1", // EnemyFactory will detect and load glorb1/2/3 for animation
                LootTable = new[]
                {
                    new LootEntry { ItemType = "brain", DropChance = 0.1f }
                }
            });
        }
    }

    /// <summary>
    /// Definição de um tipo de inimigo
    /// Princípio: Single Responsibility Principle - Armazena apenas dados de configuração
    /// </summary>
    public class EnemyDefinition
    {
        public string Name { get; set; }
        public float Health { get; set; }
        public float Damage { get; set; }
        public float Speed { get; set; }
        public float AttackCooldown { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public string TextureName { get; set; } // Opcional: nome da textura
        public LootEntry[] LootTable { get; set; }
    }

    /// <summary>
    /// Entrada na tabela de loot de um inimigo
    /// </summary>
    public class LootEntry
    {
        public string ItemType { get; set; }
        public float DropChance { get; set; }
    }
}
