using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Marca uma entidade como controlada pelo jogador
    /// </summary>
    public class PlayerInputComponent : Component
    {
        public bool IsPlayerControlled { get; set; } = true;
        public float ShootCooldown { get; set; } = 0f;

        // Tempo entre tiros (segundos). Menor = mais cadência.
        public float ShootCooldownTime { get; set; } = 0.5f;

        // Propriedades dos projéteis do jogador (modificáveis por upgrades)
        public float BulletSpeed { get; set; } = 600f;
        public float BulletDamage { get; set; } = 25f;
        public float BulletSize { get; set; } = 8f; // largura/altura do sprite do projétil
        // Número de balas adicionais por disparo (0 = tiro simples)
        public int ExtraBullets { get; set; } = 0;

        public PlayerInputComponent()
        {
        }

        public PlayerInputComponent(float bulletSpeed, float bulletDamage, float bulletSize, float shootCooldownTime = 0.5f)
        {
            BulletSpeed = bulletSpeed;
            BulletDamage = bulletDamage;
            BulletSize = bulletSize;
            ShootCooldownTime = shootCooldownTime;
        }
    }
}

