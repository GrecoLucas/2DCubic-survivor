using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor; // para GameConfig
using Microsoft.Xna.Framework;
using System;
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
    // 1. Configurações de Cores e Layout
    var overlayColor = new Color(0, 0, 0, 180);
    var panelColor = new Color(40, 44, 52); // Cinza azulado escuro (estilo moderno)
    var borderColor = Color.White * 0.2f;   // Borda sutil

    int boxW = 500, boxH = 260; // Aumentei um pouco a altura
    int centerX = GameConfig.ScreenWidth / 2;
    int centerY = GameConfig.ScreenHeight / 2;
    Rectangle box = new Rectangle(centerX - boxW / 2, centerY - boxH / 2, boxW, boxH);

    // 2. Desenhar o Fundo
    _spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), overlayColor);
    _spriteBatch.Draw(_pixelTexture, box, panelColor);
    
    // (Opcional) Desenhar uma borda na caixa principal
    _spriteBatch.Draw(_pixelTexture, new Rectangle(box.X, box.Y, boxW, 2), borderColor); // Topo
    _spriteBatch.Draw(_pixelTexture, new Rectangle(box.X, box.Y + boxH - 2, boxW, 2), borderColor); // Base
    _spriteBatch.Draw(_pixelTexture, new Rectangle(box.X, box.Y, 2, boxH), borderColor); // Esquerda
    _spriteBatch.Draw(_pixelTexture, new Rectangle(box.X + boxW - 2, box.Y, 2, boxH), borderColor); // Direita

    // 3. Título
    if (_font != null)
    {
        string title = "CHOOSE AN UPGRADE";
        Vector2 titleSize = _font.MeasureString(title);
        Vector2 titlePos = new Vector2(centerX - titleSize.X / 2, box.Y + 20);
        _spriteBatch.DrawString(_font, title, titlePos, Color.Gold);
    }

    // 4. Definição dos Botões e Lógica
    // Aqui definimos o Nome e a Ação (Lambda) de cada botão
    var input = player.GetComponent<PlayerInputComponent>();
    if (input == null) return;

    var upgrades = new[]
    {
        new { Name = "Speed (+20%)", Action = (Action)(() => input.BulletSpeed *= 1.2f) },
        new { Name = "Size (+25%)",  Action = (Action)(() => input.BulletSize *= 1.25f) },
        new { Name = "Fire Rate (+15%)", Action = (Action)(() => {
            input.ShootCooldownTime *= 0.85f;
            if (input.ShootCooldownTime < 0.05f) input.ShootCooldownTime = 0.05f;
        })}
    };

    // 5. Desenhar e Processar Botões
    var mouse = Mouse.GetState();
    bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
    
    int btnW = 400, btnH = 50;
    int startY = box.Y + 70;
    int gap = 10; // Espaço entre botões

    for (int i = 0; i < upgrades.Length; i++)
    {
        Rectangle btnRect = new Rectangle(centerX - btnW / 2, startY + (btnH + gap) * i, btnW, btnH);
        bool isHovered = btnRect.Contains(mouse.X, mouse.Y);

        // Se clicou neste botão
        if (isHovered && mouseClicked)
        {
            upgrades[i].Action.Invoke(); // Executa a lógica definida na lista acima
            System.Console.WriteLine($"[Upgrade] Aplicado: {upgrades[i].Name}");
            player.RemoveComponent<UpgradeRequestComponent>(); // Fecha o menu
            return; // Sai da função imediatamente
        }

        // Desenha o botão individual usando o helper
        DrawButton(btnRect, upgrades[i].Name, isHovered);
    }
}

// Método auxiliar para desenhar um botão bonito e centralizado
private void DrawButton(Rectangle rect, string text, bool isHovered)
{
    // Cores mudam se o mouse estiver em cima (Hover)
    Color bgColor = isHovered ? Color.CornflowerBlue : Color.DimGray;
    Color textColor = isHovered ? Color.White : Color.LightGray;

    // Fundo do botão
    _spriteBatch.Draw(_pixelTexture, rect, bgColor);

    // Texto centralizado matematicamente
    if (_font != null)
    {
        Vector2 textSize = _font.MeasureString(text);
        Vector2 textPos = new Vector2(
            rect.X + (rect.Width - textSize.X) / 2,
            rect.Y + (rect.Height - textSize.Y) / 2
        );
        _spriteBatch.DrawString(_font, text, textPos, textColor);
    }
}
    }
}