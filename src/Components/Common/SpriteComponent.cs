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
        
        /// <summary>
        /// Camada de renderização para controlar ordem de desenho.
        /// </summary>
        public RenderLayer Layer { get; set; } = RenderLayer.Entities;
        
        /// <summary>
        /// Offset de rotação para sprites que são desenhados com orientação não padrão.
        /// Exemplo: sprite facing LEFT precisa de PI radianos para apontar para direita.
        /// </summary>
        public float FacingOffsetRadians { get; set; } = 0f;

        public SpriteComponent(Color color, Vector2 size, RenderLayer layer = RenderLayer.Entities)
        {
            Color = color;
            Size = size;
            Texture = null;
            Layer = layer;
        }

        public SpriteComponent(Color color, float width, float height, RenderLayer layer = RenderLayer.Entities) 
            : this(color, new Vector2(width, height), layer)
        {
        }
        
        /// <summary>
        /// Construtor para usar textura em vez de cor sólida.
        /// </summary>
        public SpriteComponent(Texture2D texture, Vector2 size, Color? tintColor = null, RenderLayer layer = RenderLayer.Entities)
        {
            Texture = texture;
            Size = size;
            Color = Color.White; // Fallback se textura não carregar
            TintColor = tintColor ?? Color.White;
            Layer = layer;
        }
        
        public SpriteComponent(Texture2D texture, float width, float height, Color? tintColor = null, RenderLayer layer = RenderLayer.Entities)
            : this(texture, new Vector2(width, height), tintColor, layer)
        {
        }
    }
}
