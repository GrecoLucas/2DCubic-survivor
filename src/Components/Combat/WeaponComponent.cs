using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente base para armas
    /// </summary>
    public class WeaponComponent : Component
    {
        public Vector2 Size { get; set; }
        public Color Color { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }

        public WeaponComponent(Vector2 size, Color color, float offsetX = 0f, float offsetY = 0f)
        {
            Size = size;
            Color = color;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}

