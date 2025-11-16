using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de interface do usuário
    /// </summary>
    public class UISystem : GameSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixelTexture;

        public UISystem(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _pixelTexture = pixelTexture;
        }

        public override void Update(GameTime gameTime)
        {
            // Este sistema não processa na fase Update
        }

        public void Draw()
        {
            // Encontrar o jogador
            Entity player = null;
            foreach (var entity in World.GetEntitiesWithComponent<InputComponent>())
            {
                player = entity;
                break;
            }

            if (player == null) return;

            var health = player.GetComponent<HealthComponent>();
            if (health == null) return;

            _spriteBatch.Begin();

            // Posição da UI (canto inferior esquerdo)
            Vector2 position = new Vector2(20, 680);

            // Desenhar background da barra de vida
            Rectangle healthBarBg = new Rectangle(20, 680, 200, 30);
            _spriteBatch.Draw(_pixelTexture, healthBarBg, Color.DarkGray);

            // Desenhar barra de vida atual
            float healthPercent = health.CurrentHealth / health.MaxHealth;
            int healthBarWidth = (int)(196 * healthPercent);
            Rectangle healthBar = new Rectangle(22, 682, healthBarWidth, 26);
            
            Color healthColor = Color.Green;
            if (healthPercent < 0.3f) healthColor = Color.Red;
            else if (healthPercent < 0.6f) healthColor = Color.Yellow;

            _spriteBatch.Draw(_pixelTexture, healthBar, healthColor);

            // Desenhar texto de vida
            if (_font != null)
            {
                string healthText = $"HP: {(int)health.CurrentHealth}/{(int)health.MaxHealth}";
                Vector2 textPosition = new Vector2(25, 685);
                _spriteBatch.DrawString(_font, healthText, textPosition, Color.White);
            }

            _spriteBatch.End();
        }
    }
}