using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar projéteis
    /// </summary>
    public static class BulletEntity
    {
        public static Entity Create(IGameWorld world, Vector2 position, Vector2 direction, float speed = 500f, float damage = 25f)
        {
            var bullet = world.CreateEntity("Bullet");

            // Normalizar direção
            if (direction != Vector2.Zero)
                direction.Normalize();

            // Adicionar componentes
            bullet.AddComponent(new TransformComponent(position));
            bullet.AddComponent(new SpriteComponent(Color.Yellow, 8f, 8f)); // Projétil pequeno amarelo
            bullet.AddComponent(new VelocityComponent(speed));
            bullet.GetComponent<VelocityComponent>().Velocity = direction * speed;
            bullet.AddComponent(new BulletComponent(damage, 5f)); // 5 segundos de vida máxima
            bullet.AddComponent(new ColliderComponent(8f, 8f, ColliderTag.PlayerBullet));

            return bullet;
        }
    }
}

