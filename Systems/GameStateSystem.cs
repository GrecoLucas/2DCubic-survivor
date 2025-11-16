using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que gerencia o estado do jogo (game over, restart)
    /// </summary>
    public class GameStateSystem : GameSystem
    {
        public event Action OnGameOver;

       // Flag para garantir que o evento de GameOver seja disparado apenas uma vez
       private bool _gameOverRaised = false;
       public bool IsGameOver => _gameOverRaised;

       public void Reset()
       {
           _gameOverRaised = false;
       }

        public override void Update(GameTime gameTime)
        {
            // Verificar se o jogador está morto — somente se não estivermos em GameOver já
            if (_gameOverRaised) return;

            foreach (var entity in World.GetEntitiesWithComponent<InputComponent>())
            {
                var health = entity.GetComponent<HealthComponent>();
                if (health != null && !health.IsAlive)
                {
                    _gameOverRaised = true;
                    OnGameOver?.Invoke();
                    break;
                }
            }
        }
    }
}
