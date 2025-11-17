using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente para projéteis
    /// </summary>
    public class BulletComponent : Component
    {
        public float Damage { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }

        public BulletComponent(float damage, float maxLifetime = 5f)
        {
            Damage = damage;
            MaxLifetime = maxLifetime;
            Lifetime = 0f;
        }
    }
}

