using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Interface para factory de criação de projéteis
    /// </summary>
    public interface IBulletFactory
    {
        Entity CreateBullet(IGameWorld world, Vector2 position, Vector2 direction, float speed, float damage, float size = 8f);
    }
}

