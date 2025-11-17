using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Interface para regras de colisão (Strategy Pattern)
    /// </summary>
    public interface ICollisionRule
    {
        bool Matches(Entity a, Entity b);
        void Handle(Entity a, Entity b, float deltaTime, IGameWorld world);
    }
}

