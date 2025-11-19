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
    public class UISystem : GameSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixelTexture;
        private MouseState _previousMouseState;

        // Menu de upgrade (read-only existente)
        private readonly IMenu _upgradeMenu;

        // Menu principal (opcional, pode ser substituído/fechado)
        private IMenu _mainMenu;

        public UISystem(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _pixelTexture = pixelTexture;

            // Por enquanto construímos localmente; no futuro passar via construtor (DI)
            _upgradeMenu = new UpgradeMenu();
        }

        // Sobrecarga para injetar menu principal
        public UISystem(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, IMenu mainMenu)
            : this(spriteBatch, font, pixelTexture)
        {
            _mainMenu = mainMenu;
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

            if (player == null && _mainMenu == null)
                return;

            _spriteBatch.Begin();

            // Se houver menu principal, desenhá-lo e interromper UI normal enquanto estiver aberto
            if (_mainMenu != null)
            {
                bool stillOpen = _mainMenu.DrawAndHandle(player, _spriteBatch, _font, _pixelTexture, ref _previousMouseState);
                _spriteBatch.End();
                _previousMouseState = Mouse.GetState();

                if (stillOpen)
                    return; // não desenhar outras UIs enquanto o main menu estiver aberto

                // fechar menu principal após retorno falso
                _mainMenu = null;
                return;
            }

            if (player == null)
            {
                _spriteBatch.End();
                _previousMouseState = Mouse.GetState();
                return;
            }

            var health = player.GetComponent<HealthComponent>();
            var xp = player.GetComponent<XpComponent>();
            if (health == null) 
            {
                _spriteBatch.End();
                _previousMouseState = Mouse.GetState();
                return;
            }

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
                // Delegação para a nova classe (tratamento de input e desenho centralizado)
                _upgradeMenu.DrawAndHandle(player, _spriteBatch, _font, _pixelTexture, ref _previousMouseState);
            }

            _spriteBatch.End();

            _previousMouseState = Mouse.GetState();
        }
    }
}