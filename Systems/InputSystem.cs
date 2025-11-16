using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que processa input do teclado para entidades controláveis
    /// </summary>
    public class InputSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            foreach (var entity in World.GetEntitiesWithComponent<InputComponent>())
            {
                var input = entity.GetComponent<InputComponent>();
                var velocity = entity.GetComponent<VelocityComponent>();

                if (input == null || velocity == null || !input.Enabled)
                    continue;

                // Calcular direção baseada em WASD
                var direction = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.W))
                    direction.Y -= 1;
                if (keyboardState.IsKeyDown(Keys.S))
                    direction.Y += 1;
                if (keyboardState.IsKeyDown(Keys.A))
                    direction.X -= 1;
                if (keyboardState.IsKeyDown(Keys.D))
                    direction.X += 1;

                // Normalizar para movimento diagonal não ser mais rápido
                if (direction != Vector2.Zero)
                    direction.Normalize();

                velocity.Velocity = direction * velocity.Speed;
            }
        }
    }
}
