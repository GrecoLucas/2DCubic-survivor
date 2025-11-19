using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de renderização (cor e tamanho para formas simples, ou textura)
    /// </summary>
    public class SpriteComponent : Component
    {
        public Color Color { get; set; }
        public Vector2 Size { get; set; }
        public Texture2D Texture { get; set; }

        public SpriteComponent(Color color, Vector2 size)
        {
            Color = color;
            Size = size;
            Texture = null;
        }

        public SpriteComponent(Color color, float width, float height) 
            : this(color, new Vector2(width, height))
        {
        }
        
        public SpriteComponent(Texture2D texture, float width, float height)
        {
            Texture = texture;
            Color = Microsoft.Xna.Framework.Color.White;
            Size = new Vector2(width, height);
        }
    }
}
