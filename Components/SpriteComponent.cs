using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de renderização (cor e tamanho para formas simples)
    /// </summary>
    public class SpriteComponent : Component
    {
        public Color Color { get; set; }
        public Vector2 Size { get; set; }

        public SpriteComponent(Color color, Vector2 size)
        {
            Color = color;
            Size = size;
        }

        public SpriteComponent(Color color, float width, float height) 
            : this(color, new Vector2(width, height))
        {
        }
    }
}
