using System;
using CubeSurvivor.Core.Registry;
using CubeSurvivor.World.Biomes;

namespace CubeSurvivor.Game.Registries
{
    /// <summary>
    /// Registry para definições de biomas
    /// Princípio: Single Responsibility Principle (SRP) - Gerencia apenas biomas
    /// Princípio: Open/Closed Principle (OCP) - Aberto para extensão com novos biomas
    /// 
    /// Uso: Adicionar novos biomas sem modificar código existente
    /// Exemplo:
    /// <code>
    /// var definition = new BiomeDefinition
    /// {
    ///     Type = BiomeType.Desert,
    ///     AllowsEnemySpawns = true,
    ///     TreeDensity = 5,
    ///     GoldDensity = 20,
    ///     TextureName = "desert.png"
    /// };
    /// BiomeRegistry.Instance.Register("desert", definition);
    /// </code>
    /// </summary>
    public sealed class BiomeRegistry : Registry<string, BiomeDefinition>
    {
        private static readonly Lazy<BiomeRegistry> _instance = 
            new Lazy<BiomeRegistry>(() => new BiomeRegistry());

        public static BiomeRegistry Instance => _instance.Value;

        private BiomeRegistry()
        {
            RegisterDefaultBiomes();
        }

        private void RegisterDefaultBiomes()
        {
            // Bioma Floresta
            Register("forest", new BiomeDefinition
            {
                Type = BiomeType.Forest,
                AllowsEnemySpawns = false,
                TreeDensity = 40,
                GoldDensity = 0,
                TextureName = "grass.png"
            });

            // Bioma Caverna
            Register("cave", new BiomeDefinition
            {
                Type = BiomeType.Cave,
                AllowsEnemySpawns = true,
                TreeDensity = 0,
                GoldDensity = 40,
                TextureName = "cave.png"
            });
        }
    }

    /// <summary>
    /// Definição de um bioma
    /// Princípio: Single Responsibility Principle - Armazena apenas dados de configuração
    /// </summary>
    public class BiomeDefinition
    {
        public BiomeType Type { get; set; }
        public bool AllowsEnemySpawns { get; set; }
        public int TreeDensity { get; set; }
        public int GoldDensity { get; set; }
        public string TextureName { get; set; }
    }
}
