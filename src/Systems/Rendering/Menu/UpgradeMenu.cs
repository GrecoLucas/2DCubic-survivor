using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CubeSurvivor.Systems
{
    // Interface para menus (aberto a novas implementações)
    public interface IMenu
    {
        /// Desenha e processa input do menu. Retorna true se o menu ainda está aberto.
        bool DrawAndHandle(Entity player, SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, ref MouseState previousMouseState);
    }

    // Implementação do menu de upgrade (separada para reutilização)
    public sealed class UpgradeMenu : IMenu
    {
        private readonly Texture2D _brainTexture;
        private int _page = 1;
        
        // Tamanho dinâmico da tela
        private int _screenWidth = GameConfig.ScreenWidth;
        private int _screenHeight = GameConfig.ScreenHeight;

        public UpgradeMenu(Texture2D brainTexture = null)
        {
            _brainTexture = brainTexture;
        }

        /// <summary>
        /// Atualiza o tamanho da tela para posicionamento dinâmico.
        /// </summary>
        public void SetScreenSize(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
        }

        public bool DrawAndHandle(Entity player, SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture, ref MouseState previousMouseState)
        {
            if (player == null || pixelTexture == null) return false;

            // Obter componente de XP para verificar pontos disponíveis
            var xp = player.GetComponent<XpComponent>();
            if (xp == null || xp.AvailableUpgradePoints <= 0)
            {
                // Se não há pontos, fechar o menu
                player.RemoveComponent<UpgradeRequestComponent>();
                return false;
            }

            // 1. Cores e layout
            var overlayColor = new Color(0, 0, 0, 180);
            var panelColor = new Color(40, 44, 52);
            var borderColor = Color.White * 0.2f;

            int boxW = 500, boxH = 340;
            int centerX = _screenWidth / 2;
            int centerY = _screenHeight / 2;
            Rectangle box = new Rectangle(centerX - boxW / 2, centerY - boxH / 2, boxW, boxH);

            // 2. Desenhar fundo e painel
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, _screenWidth, _screenHeight), overlayColor);
            spriteBatch.Draw(pixelTexture, box, panelColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y, boxW, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y + boxH - 2, boxW, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y, 2, boxH), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X + boxW - 2, box.Y, 2, boxH), borderColor);

            // 3. Título com contador de pontos e navegação de páginas
            if (font != null)
            {
                string title = "CHOOSE AN UPGRADE";
                Vector2 titleSize = font.MeasureString(title);
                Vector2 titlePos = new Vector2(centerX - titleSize.X / 2, box.Y + 12);
                spriteBatch.DrawString(font, title, titlePos, Color.Gold);

                // Mostrar pontos disponíveis
                string pointsText = $"Pontos disponíveis: {xp.AvailableUpgradePoints}";
                Vector2 pointsSize = font.MeasureString(pointsText);
                Vector2 pointsPos = new Vector2(centerX - pointsSize.X / 2, box.Y + 42);
                spriteBatch.DrawString(font, pointsText, pointsPos, Color.White);

                // Mostrar qual página está ativa
                string pageText = $"Página {_page}/2";
                Vector2 pageSize = font.MeasureString(pageText);
                Vector2 pagePos = new Vector2(box.X + boxW - pageSize.X - 12, box.Y + 12);
                spriteBatch.DrawString(font, pageText, pagePos, Color.LightGray);
            }

            // 4. Lógica dos upgrades (SRP: responsabilidade aqui é só aplicar upgrades)
            var input = player.GetComponent<PlayerInputComponent>();
            if (input == null) return true; // manter menu aberto se não houver input

            // Dependendo da página, montar lista de upgrades
            object[] upgradesPage1 = new[]
            {
                new { Name = "Speed (+20%)", Action = (Action)(() => input.BulletSpeed *= 1.2f) },
                new { Name = "Size (+25%)",  Action = (Action)(() => input.BulletSize *= 1.25f) },
                new { Name = "Fire Rate (+15%)", Action = (Action)(() => {
                    input.ShootCooldownTime *= 0.85f;
                    if (input.ShootCooldownTime < 0.05f) input.ShootCooldownTime = 0.05f;
                })}
            };

            var upgradesPage2 = new[]
            {
                new { Name = "Increase Max Health (+25%)", Action = (Action)(() => {
                    var health = player.GetComponent<HealthComponent>();
                    if (health != null)
                    {
                        health.MaxHealth *= 1.25f;
                        health.CurrentHealth = health.MaxHealth;
                    }
                }) },
                new { Name = "Increase Speed (+15%)", Action = (Action)(() => {
                    var vel = player.GetComponent<VelocityComponent>();
                    if (vel != null) vel.Speed *= 1.15f;
                }) },
                // Third entry (bullets) will be handled specially below because it costs brains
                new { Name = "Increase Bullets (special)", Action = (Action)(() => { }) }
            };

            object[] currentUpgrades = _page == 1 ? upgradesPage1 : upgradesPage2;

            // 5. Desenhar e processar botões
            var mouse = Mouse.GetState();
            bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;

            int btnW = 400, btnH = 50;
            int startY = box.Y + 80;
            int gap = 10;

            // Desenhar botões de upgrade (com tratamento especial para página 2, item 3)
            for (int i = 0; i < currentUpgrades.Length; i++)
            {
                Rectangle btnRect = new Rectangle(centerX - btnW / 2, startY + (btnH + gap) * i, btnW, btnH);
                bool isHovered = btnRect.Contains(mouse.X, mouse.Y);

                // Se estamos na página 2 e este é o terceiro elemento, desenhar botão especial
                if (_page == 2 && i == 2)
                {
                    // Determinar custo atual (próximo slot custa ExtraBullets+1 cérebros)
                    int currentExtra = input.ExtraBullets;
                    int cost = currentExtra + 1;

                    // Contar cérebros no inventário do jogador
                    int brainsOwned = 0;
                    var invComp = player.GetComponent<CubeSurvivor.Inventory.Components.InventoryComponent>();
                    if (invComp != null)
                    {
                        foreach (var stack in invComp.Inventory.GetAllStacks())
                        {
                            if (stack != null && stack.Item != null && stack.Item.Id == "brain")
                                brainsOwned += stack.Quantity;
                        }
                    }

                    // Texto do botão com ícone de cérebro e contagem
                    string buttonText = $"Increase Bullets (+1)  ";
                    DrawButton(spriteBatch, pixelTexture, font, btnRect, buttonText, isHovered);

                    // Desenhar ícone de cérebro à direita do botão
                    if (_brainTexture != null)
                    {
                        int iconSize = btnH - 10;
                        Rectangle iconRect = new Rectangle(btnRect.Right - iconSize - 10, btnRect.Y + 5, iconSize, iconSize);
                        spriteBatch.Draw(_brainTexture, iconRect, Color.White);
                        if (font != null)
                        {
                            string countText = $"({brainsOwned}/{cost})";
                            Vector2 ctSize = font.MeasureString(countText);
                            Vector2 ctPos = new Vector2(iconRect.X - ctSize.X - 6, btnRect.Y + (btnH - ctSize.Y) / 2);
                            spriteBatch.DrawString(font, countText, ctPos, Color.White);
                        }
                    }

                    // Processar clique (usar cérebros em vez de pontos)
                    if (isHovered && mouseClicked)
                    {
                        if (brainsOwned >= cost && invComp != null)
                        {
                            // Remover cérebros do inventário e aplicar upgrade
                            bool removed = invComp.Inventory.RemoveItem("brain", cost);
                            if (removed)
                            {
                                input.ExtraBullets++;
                                System.Console.WriteLine($"[Upgrade] +1 bullet aplicado. ExtraBullets={input.ExtraBullets}");
                            }
                            else
                            {
                                System.Console.WriteLine("[Upgrade] Falha ao remover cérebros do inventário");
                            }
                        }
                        else
                        {
                            System.Console.WriteLine($"[Upgrade] Cérebros insuficientes ({brainsOwned}/{cost})");
                        }

                        previousMouseState = mouse;
                        // Não consome pontos normais; manter menu aberto
                        return true;
                    }

                    continue;
                }

                // Página normal: gastar pontos de upgrade
                if (isHovered && mouseClicked)
                {
                    // invocar ação dinamicamente
                    var dyn = currentUpgrades[i];
                    var action = dyn.GetType().GetProperty("Action").GetValue(dyn) as Action;
                    action?.Invoke();
                    xp.AvailableUpgradePoints--;
                    System.Console.WriteLine($"[Upgrade] Aplicado: {dyn.GetType().GetProperty("Name").GetValue(dyn)} (Pontos restantes: {xp.AvailableUpgradePoints})");

                    if (xp.AvailableUpgradePoints <= 0)
                    {
                        player.RemoveComponent<UpgradeRequestComponent>();
                        xp.HasPendingLevelUp = false;
                        previousMouseState = mouse;
                        return false; // menu fechado
                    }

                    previousMouseState = mouse;
                    return true; // menu continua aberto
                }

                // Desenhar botão normalmente
                string label = currentUpgrades[i].GetType().GetProperty("Name").GetValue(currentUpgrades[i]).ToString();
                DrawButton(spriteBatch, pixelTexture, font, btnRect, label, isHovered);
            }
            
            // Botão "Fechar Menu" no final e setas de paginação ao redor
            int closeY = startY + (btnH + gap) * currentUpgrades.Length + 10;
            Rectangle closeBtnRect = new Rectangle(centerX - btnW / 2, closeY, btnW, btnH);
            bool closeHovered = closeBtnRect.Contains(mouse.X, mouse.Y);

            // Desenhar setas estilizadas ao lado do botão de saída
            int arrowW = 40, arrowH = btnH;
            Rectangle leftArrowRect = new Rectangle(closeBtnRect.X - arrowW - 8, closeBtnRect.Y, arrowW, arrowH);
            Rectangle rightArrowRect = new Rectangle(closeBtnRect.X + closeBtnRect.Width + 8, closeBtnRect.Y, arrowW, arrowH);

            bool leftHovered = leftArrowRect.Contains(mouse.X, mouse.Y) && _page > 1;
            bool rightHovered = rightArrowRect.Contains(mouse.X, mouse.Y) && _page < 2;

            Color arrowBg = Color.SaddleBrown;
            Color arrowHoverBg = Color.Gold;
            // desenhar fundos
            spriteBatch.Draw(pixelTexture, leftArrowRect, leftHovered ? arrowHoverBg : arrowBg);
            spriteBatch.Draw(pixelTexture, rightArrowRect, rightHovered ? arrowHoverBg : arrowBg);

            // desenhar símbolos
            if (font != null)
            {
                // Use ASCII arrows to avoid missing glyphs in the SpriteFont
                string leftSym = "<";
                string rightSym = ">";
                Vector2 lSize = font.MeasureString(leftSym);
                Vector2 rSize = font.MeasureString(rightSym);
                spriteBatch.DrawString(font, leftSym, new Vector2(leftArrowRect.X + (leftArrowRect.Width - lSize.X) / 2, leftArrowRect.Y + (leftArrowRect.Height - lSize.Y) / 2), Color.White);
                spriteBatch.DrawString(font, rightSym, new Vector2(rightArrowRect.X + (rightArrowRect.Width - rSize.X) / 2, rightArrowRect.Y + (rightArrowRect.Height - rSize.Y) / 2), Color.White);
            }

            // agora o botão de fechar
            if (closeHovered && mouseClicked)
            {
                player.RemoveComponent<UpgradeRequestComponent>();
                // Manter flag HasPendingLevelUp true se ainda houver pontos
                xp.HasPendingLevelUp = xp.AvailableUpgradePoints > 0;
                System.Console.WriteLine($"[Upgrade] Menu fechado (Pontos não gastos: {xp.AvailableUpgradePoints})");
                previousMouseState = mouse;
                return false; // menu fechado
            }

            DrawButton(spriteBatch, pixelTexture, font, closeBtnRect, "Fechar Menu", closeHovered, Color.Gray, Color.DarkGray);

            // Processar clique das setas (usar mouseClicked)
            if (mouseClicked)
            {
                if (leftHovered && _page > 1)
                {
                    _page = Math.Max(1, _page - 1);
                    previousMouseState = mouse;
                    return true; // manter menu aberto
                }
                if (rightHovered && _page < 2)
                {
                    _page = Math.Min(2, _page + 1);
                    previousMouseState = mouse;
                    return true; // manter menu aberto
                }
            }

            // Atualizar estado do mouse para a próxima chamada
            previousMouseState = mouse;
            return true; // menu continua aberto
        }

        // Helper local para desenhar botões (pode ser extraído para utilitária se desejar)
        private void DrawButton(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont font, Rectangle rect, string text, bool isHovered, Color? normalColor = null, Color? hoverColor = null)
        {
            Color bgColor = isHovered ? (hoverColor ?? Color.CornflowerBlue) : (normalColor ?? Color.DimGray);
            Color textColor = isHovered ? Color.White : Color.LightGray;

            spriteBatch.Draw(pixelTexture, rect, bgColor);

            if (font != null)
            {
                Vector2 textSize = font.MeasureString(text);
                Vector2 textPos = new Vector2(
                    rect.X + (rect.Width - textSize.X) / 2,
                    rect.Y + (rect.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, text, textPos, textColor);
            }
        }
    }
}