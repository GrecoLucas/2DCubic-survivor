using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Interface para factory de criação do jogador
    /// </summary>
    public interface IPlayerFactory
    {
        Entity CreatePlayer(IGameWorld world, Vector2 position);
    }
}

