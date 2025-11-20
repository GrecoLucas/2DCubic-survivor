using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Marks the muzzle position on a gun visual for bullet spawning
    /// </summary>
    public sealed class GunMuzzleComponent : Component
    {
        public Vector2 LocalMuzzleOffset { get; } // in gun world units

        public GunMuzzleComponent(Vector2 localOffset)
        {
            LocalMuzzleOffset = localOffset;
        }
    }
}

