using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de transformação (posição, rotação, escala)
    /// </summary>
    public class TransformComponent : Component
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }

        public TransformComponent(Vector2 position)
        {
            Position = position;
            Rotation = 0f;
            Scale = Vector2.One;
        }

        public TransformComponent(float x, float y) : this(new Vector2(x, y))
        {
        }
    }
}
