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
        
        /// <summary>
        /// Cor de tint para aplicar sobre a textura (ou cor sólida se não houver textura).
        /// Padrão: Color.White (sem tint).
        /// </summary>
        public Color TintColor { get; set; } = Color.White;

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
        
        /// <summary>
        /// Construtor para usar textura em vez de cor sólida.
        /// </summary>
        public SpriteComponent(Texture2D texture, Vector2 size, Color? tintColor = null)
        {
            Texture = texture;
            Size = size;
            Color = Color.White; // Fallback se textura não carregar
            TintColor = tintColor ?? Color.White;
        }
        
        public SpriteComponent(Texture2D texture, float width, float height, Color? tintColor = null)
            : this(texture, new Vector2(width, height), tintColor)
        {
        }
    }
}
