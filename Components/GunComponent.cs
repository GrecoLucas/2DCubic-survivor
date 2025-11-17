using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente específico para arma de fogo (pistola/rifle)
    /// </summary>
    public class GunComponent : WeaponComponent
    {
        public GunComponent() 
            : base(new Vector2(25f, 6f), Color.Black, 0f, 0f)
        {
        }
    }
}

