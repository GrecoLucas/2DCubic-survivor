using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de colisão com bounds retangular
    /// </summary>
    public class ColliderComponent : Component
    {
        public Vector2 Size { get; set; }
        public bool IsTrigger { get; set; }
        public ColliderTag Tag { get; set; }

        public ColliderComponent(Vector2 size, ColliderTag tag = ColliderTag.Default)
        {
            Size = size;
            Tag = tag;
            IsTrigger = false;
        }

        public ColliderComponent(float width, float height, ColliderTag tag = ColliderTag.Default) 
            : this(new Vector2(width, height), tag)
        {
        }

        public Rectangle GetBounds(Vector2 position)
        {
            return new Rectangle(
                (int)(position.X - Size.X / 2),
                (int)(position.Y - Size.Y / 2),
                (int)Size.X,
                (int)Size.Y
            );
        }
    }
}