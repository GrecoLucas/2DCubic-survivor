using Microsoft.Xna.Framework;

namespace CubeSurvivor.Game.States
{
    /// <summary>
    /// Interface for game states (menu, play, editor).
    /// Allows switching between different game modes.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Called when entering this state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Updates the state logic.
        /// </summary>
        void Update(GameTime gameTime);

        /// <summary>
        /// Draws the state.
        /// </summary>
        void Draw(GameTime gameTime);
    }
}





