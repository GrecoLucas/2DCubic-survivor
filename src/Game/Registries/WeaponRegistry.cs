using System;
using CubeSurvivor.Core.Registry;

namespace CubeSurvivor.Game.Registries
{
    /// <summary>
    /// Registry para definições de armas
    /// Princípio: Single Responsibility Principle (SRP) - Gerencia apenas definições de armas
    /// Princípio: Open/Closed Principle (OCP) - Aberto para novos tipos de armas
    /// 
    /// Uso: Adicionar novas armas facilmente
    /// Exemplo:
    /// <code>
    /// var shotgunDefinition = new WeaponDefinition
    /// {
    ///     Name = "Shotgun",
    ///     Damage = 15f,
    ///     FireRate = 0.8f,
    ///     BulletSpeed = 400f,
    ///     BulletSize = 6f,
    ///     Width = 30f,
    ///     Height = 8f,
    ///     TextureName = "shotgun.png",
    ///     BulletsPerShot = 5,
    ///     Spread = 30f
    /// };
    /// WeaponRegistry.Instance.Register("shotgun", shotgunDefinition);
    /// </code>
    /// </summary>
    public sealed class WeaponRegistry : Registry<string, WeaponDefinition>
    {
        private static readonly Lazy<WeaponRegistry> _instance = 
            new Lazy<WeaponRegistry>(() => new WeaponRegistry());

        public static WeaponRegistry Instance => _instance.Value;

        private WeaponRegistry()
        {
            RegisterDefaultWeapons();
        }

        private void RegisterDefaultWeapons()
        {
            // Gun (arma padrão)
            Register("gun", new WeaponDefinition
            {
                Name = "Gun",
                Damage = 10f,
                FireRate = 0.5f, // 2 tiros por segundo
                BulletSpeed = 500f,
                BulletSize = 5f,
                Width = 25f,
                Height = 6f,
                TextureName = "gun.png",
                BulletsPerShot = 1,
                Spread = 0f
            });

            // Exemplo de pistola rápida
            Register("pistol", new WeaponDefinition
            {
                Name = "Pistol",
                Damage = 8f,
                FireRate = 0.3f,
                BulletSpeed = 600f,
                BulletSize = 4f,
                Width = 20f,
                Height = 5f,
                TextureName = "pistol.png",
                BulletsPerShot = 1,
                Spread = 2f
            });

            // Exemplo de rifle de precisão
            Register("rifle", new WeaponDefinition
            {
                Name = "Rifle",
                Damage = 25f,
                FireRate = 1.0f,
                BulletSpeed = 800f,
                BulletSize = 6f,
                Width = 35f,
                Height = 7f,
                TextureName = "rifle.png",
                BulletsPerShot = 1,
                Spread = 0f
            });
        }
    }

    /// <summary>
    /// Definição de uma arma
    /// Princípio: Single Responsibility Principle - Armazena apenas dados de configuração
    /// </summary>
    public class WeaponDefinition
    {
        public string Name { get; set; }
        public float Damage { get; set; }
        public float FireRate { get; set; }
        public float BulletSpeed { get; set; }
        public float BulletSize { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string TextureName { get; set; }
        public int BulletsPerShot { get; set; } = 1;
        public float Spread { get; set; } = 0f; // Dispersão em graus
        public byte ColorR { get; set; } = 0;
        public byte ColorG { get; set; } = 0;
        public byte ColorB { get; set; } = 0;
    }
}
