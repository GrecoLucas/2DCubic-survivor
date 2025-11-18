using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor; // para GameConfig
using Microsoft.Xna.Framework;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        private MouseState _previousMouseState;

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
            foreach (var entity in World.GetEntitiesWithComponent<PlayerInputComponent>())
            {
                player = entity;
                break;
            }

            if (player == null) return;

            var health = player.GetComponent<HealthComponent>();
            var xp = player.GetComponent<XpComponent>();
            if (health == null) return;

            _spriteBatch.Begin();

            // Posição da UI (canto inferior esquerdo) - barra de vida
            Vector2 position = new Vector2(20, GameConfig.ScreenHeight - 40); // ajustado usando GameConfig

            // Desenhar background da barra de vida
            Rectangle healthBarBg = new Rectangle(20, GameConfig.ScreenHeight - 40, 200, 30);
            _spriteBatch.Draw(_pixelTexture, healthBarBg, Color.DarkGray);

            // Desenhar barra de vida atual
            float healthPercent = health.CurrentHealth / health.MaxHealth;
            int healthBarWidth = (int)(196 * healthPercent);
            Rectangle healthBar = new Rectangle(22, GameConfig.ScreenHeight - 38, healthBarWidth, 26);

            Color healthColor = Color.Green;
            if (healthPercent < 0.3f) healthColor = Color.Red;
            else if (healthPercent < 0.6f) healthColor = Color.Yellow;

            _spriteBatch.Draw(_pixelTexture, healthBar, healthColor);

            // Desenhar texto de vida
            if (_font != null)
            {
                string healthText = $"HP: {(int)health.CurrentHealth}/{(int)health.MaxHealth}";
                Vector2 textPosition = new Vector2(25, GameConfig.ScreenHeight - 35);
                _spriteBatch.DrawString(_font, healthText, textPosition, Color.White);
            }

            // Barra de XP (oposto da vida) - canto inferior direito
            if (xp != null)
            {
                Rectangle xpBarBg = new Rectangle(GameConfig.ScreenWidth - 220, GameConfig.ScreenHeight - 40, 200, 30);
                _spriteBatch.Draw(_pixelTexture, xpBarBg, Color.DarkGray);

                float xpPercent = xp.RequiredXp > 0 ? xp.CurrentXp / xp.RequiredXp : 0f;
                int xpBarWidth = (int)(196 * xpPercent);
                Rectangle xpBar = new Rectangle(GameConfig.ScreenWidth - 218, GameConfig.ScreenHeight - 38, xpBarWidth, 26);

                _spriteBatch.Draw(_pixelTexture, xpBar, Color.CornflowerBlue);

                if (_font != null)
                {
                    string xpText = $"XP: {(int)xp.CurrentXp}/{(int)xp.RequiredXp}";
                    Vector2 xpTextPos = new Vector2(GameConfig.ScreenWidth - 215, GameConfig.ScreenHeight - 35);
                    _spriteBatch.DrawString(_font, xpText, xpTextPos, Color.White);
                }
            }

            // Se houver um pedido de upgrade, desenhar o menu e processar cliques
            var upgradeReq = World.GetEntitiesWithComponent<UpgradeRequestComponent>().FirstOrDefault();
            if (upgradeReq != null)
            {
                DrawUpgradeMenu(player);
            }

            _spriteBatch.End();

            _previousMouseState = Mouse.GetState();
        }

        private void DrawUpgradeMenu(Entity player)
        {
            // Overlay semi-transparente
            Rectangle full = new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight);
            _spriteBatch.Draw(_pixelTexture, full, new Color(0, 0, 0, 180));

            // Caixa central
            int boxW = 500, boxH = 220;
            int boxX = (GameConfig.ScreenWidth - boxW) / 2;
            int boxY = (GameConfig.ScreenHeight - boxH) / 2;
            Rectangle box = new Rectangle(boxX, boxY, boxW, boxH);
            _spriteBatch.Draw(_pixelTexture, box, Color.DarkSlateGray);

            // Título
            if (_font != null)
            {
                var title = "Choose an Upgrade";
                Vector2 titlePos = new Vector2(boxX + 20, boxY + 12);
                _spriteBatch.DrawString(_font, title, titlePos, Color.White);
            }

            // Botões (3 opções)
            int btnW = 420, btnH = 48;
            int btnX = boxX + 40;
            int btnY = boxY + 60;
            Rectangle btnSpeed = new Rectangle(btnX, btnY, btnW, btnH);
            Rectangle btnSize = new Rectangle(btnX, btnY + 56, btnW, btnH);
            Rectangle btnRate = new Rectangle(btnX, btnY + 112, btnW, btnH);

            _spriteBatch.Draw(_pixelTexture, btnSpeed, Color.DimGray);
            _spriteBatch.Draw(_pixelTexture, btnSize, Color.DimGray);
            _spriteBatch.Draw(_pixelTexture, btnRate, Color.DimGray);

            if (_font != null)
            {
                _spriteBatch.DrawString(_font, "Projectile Speed", new Vector2(btnSpeed.X + 12, btnSpeed.Y + 12), Color.White);
                _spriteBatch.DrawString(_font, "Projectile Size", new Vector2(btnSize.X + 12, btnSize.Y + 12), Color.White);
                _spriteBatch.DrawString(_font, "Fire Rate (Cadence)", new Vector2(btnRate.X + 12, btnRate.Y + 12), Color.White);
            }

            // Processar clique do mouse (apenas clique esquerdo novo)
            var mouse = Mouse.GetState();
            bool clicked = mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
            if (clicked && player != null)
            {
                var input = player.GetComponent<PlayerInputComponent>();
                if (input != null)
                {
                    bool applied = false;
                    if (btnSpeed.Contains(mouse.X, mouse.Y))
                    {
                        var old = input.BulletSpeed;
                        input.BulletSpeed *= 1.2f; // +20% speed
                        System.Console.WriteLine($"[Melhoria] Velocidade do projétil era {old:F1} virou {input.BulletSpeed:F1}");
                        applied = true;
                    }
                    else if (btnSize.Contains(mouse.X, mouse.Y))
                    {
                        var old = input.BulletSize;
                        input.BulletSize *= 1.25f; // +25% size
                        System.Console.WriteLine($"[Melhoria] Tamanho do projétil era {old:F1} virou {input.BulletSize:F1}");
                        applied = true;
                    }
                    else if (btnRate.Contains(mouse.X, mouse.Y))
                    {
                        var old = input.ShootCooldownTime;
                        input.ShootCooldownTime *= 0.85f; // -15% cooldown (faster fire)
                        if (input.ShootCooldownTime < 0.05f) input.ShootCooldownTime = 0.05f;
                        System.Console.WriteLine($"[Melhoria] Tempo de recarga era {old:F2}s virou {input.ShootCooldownTime:F2}s");
                        applied = true;
                    }

                    // Remover o pedido de upgrade (menu fechado) e retomar o jogo somente se uma opção foi escolhida
                    if (applied)
                    {
                        player.RemoveComponent<UpgradeRequestComponent>();
                    }
                }
            }
        }
    }
}