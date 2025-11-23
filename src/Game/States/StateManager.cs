using System;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Game.States
{
    /// <summary>
    /// Manages game states and handles transitions between them.
    /// </summary>
    public sealed class StateManager
    {
        public IGameState CurrentState { get; private set; }

        /// <summary>
        /// Switches to a new state, calling Exit on the old state and Enter on the new one.
        /// </summary>
        /// <param name="newState">The state to switch to.</param>
        public void SwitchState(IGameState newState)
        {
            if (newState == null)
            {
                throw new ArgumentNullException(nameof(newState));
            }

            // Exit current state
            CurrentState?.Exit();

            // Switch to new state
            CurrentState = newState;

            // Enter new state
            CurrentState.Enter();

            Console.WriteLine($"[StateManager] Switched to {newState.GetType().Name}");
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            CurrentState?.Update(gameTime);
        }

        /// <summary>
        /// Draws the current state.
        /// </summary>
        public void Draw(GameTime gameTime)
        {
            CurrentState?.Draw(gameTime);
        }
    }
}





