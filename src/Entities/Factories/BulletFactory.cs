using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar projéteis
    /// </summary>
    public sealed class BulletFactory : IBulletFactory
    {
        private readonly TextureManager _textureManager;

        public BulletFactory(TextureManager textureManager = null)
        {
            _textureManager = textureManager;
        }

        public Entity CreateBullet(IGameWorld world, Vector2 position, Vector2 direction, float speed, float damage, float size = 8f)
        {
            var bullet = world.CreateEntity("Bullet");

            // Normalizar direção
            if (direction != Vector2.Zero)
                direction.Normalize();

            // Adicionar componentes
            bullet.AddComponent(new TransformComponent(position));
            
            // Projéteis usam a camada Projectiles
            // Try to use texture if available, fallback to color
            Texture2D bulletTexture = _textureManager?.GetTexture("bullet");
            if (bulletTexture != null)
            {
                bullet.AddComponent(new SpriteComponent(bulletTexture, size, size, null, RenderLayer.Projectiles));
            }
            else
            {
                bullet.AddComponent(new SpriteComponent(Color.Yellow, size, size, RenderLayer.Projectiles));
            }
            
            bullet.AddComponent(new VelocityComponent(speed));
            bullet.GetComponent<VelocityComponent>().Velocity = direction * speed;
            bullet.AddComponent(new BulletComponent(damage, 5f)); // 5 segundos de vida máxima
            bullet.AddComponent(new ColliderComponent(size, size, ColliderTag.PlayerBullet));

            return bullet;
        }
    }
}

