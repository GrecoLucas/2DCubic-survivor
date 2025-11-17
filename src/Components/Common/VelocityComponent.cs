using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de velocidade para movimento
    /// </summary>
    public class VelocityComponent : Component
    {
        public Vector2 Velocity { get; set; }
        public float Speed { get; set; }

        public VelocityComponent(float speed = 200f)
        {
            Speed = speed;
            Velocity = Vector2.Zero;
        }
    }
}
