using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar projéteis
    /// </summary>
    public sealed class BulletFactory : IBulletFactory
    {
        public Entity CreateBullet(IGameWorld world, Vector2 position, Vector2 direction, float speed, float damage, float size = 8f)
        {
            var bullet = world.CreateEntity("Bullet");

            // Normalizar direção
            if (direction != Vector2.Zero)
                direction.Normalize();

            // Adicionar componentes
            bullet.AddComponent(new TransformComponent(position));
            // Projéteis usam a camada Projectiles
            bullet.AddComponent(new SpriteComponent(Color.Yellow, size, size, RenderLayer.Projectiles));
            bullet.AddComponent(new VelocityComponent(speed));
            bullet.GetComponent<VelocityComponent>().Velocity = direction * speed;
            bullet.AddComponent(new BulletComponent(damage, 5f)); // 5 segundos de vida máxima
            bullet.AddComponent(new ColliderComponent(size, size, ColliderTag.PlayerBullet));

            return bullet;
        }
    }
}

