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
            int centerX = GameConfig.ScreenWidth / 2;
            int centerY = GameConfig.ScreenHeight / 2;
            Rectangle box = new Rectangle(centerX - boxW / 2, centerY - boxH / 2, boxW, boxH);

            // 2. Desenhar fundo e painel
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, GameConfig.ScreenWidth, GameConfig.ScreenHeight), overlayColor);
            spriteBatch.Draw(pixelTexture, box, panelColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y, boxW, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y + boxH - 2, boxW, 2), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X, box.Y, 2, boxH), borderColor);
            spriteBatch.Draw(pixelTexture, new Rectangle(box.X + boxW - 2, box.Y, 2, boxH), borderColor);

            // 3. Título com contador de pontos
            if (font != null)
            {
                string title = "CHOOSE AN UPGRADE";
                Vector2 titleSize = font.MeasureString(title);
                Vector2 titlePos = new Vector2(centerX - titleSize.X / 2, box.Y + 15);
                spriteBatch.DrawString(font, title, titlePos, Color.Gold);
                
                // Mostrar pontos disponíveis
                string pointsText = $"Pontos disponíveis: {xp.AvailableUpgradePoints}";
                Vector2 pointsSize = font.MeasureString(pointsText);
                Vector2 pointsPos = new Vector2(centerX - pointsSize.X / 2, box.Y + 45);
                spriteBatch.DrawString(font, pointsText, pointsPos, Color.White);
            }

            // 4. Lógica dos upgrades (SRP: responsabilidade aqui é só aplicar upgrades)
            var input = player.GetComponent<PlayerInputComponent>();
            if (input == null) return true; // manter menu aberto se não houver input

            var upgrades = new[]
            {
                new { Name = "Speed (+20%)", Action = (Action)(() => input.BulletSpeed *= 1.2f) },
                new { Name = "Size (+25%)",  Action = (Action)(() => input.BulletSize *= 1.25f) },
                new { Name = "Fire Rate (+15%)", Action = (Action)(() => {
                    input.ShootCooldownTime *= 0.85f;
                    if (input.ShootCooldownTime < 0.05f) input.ShootCooldownTime = 0.05f;
                })}
            };

            // 5. Desenhar e processar botões
            var mouse = Mouse.GetState();
            bool mouseClicked = mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;

            int btnW = 400, btnH = 50;
            int startY = box.Y + 80;
            int gap = 10;

            // Desenhar botões de upgrade
            for (int i = 0; i < upgrades.Length; i++)
            {
                Rectangle btnRect = new Rectangle(centerX - btnW / 2, startY + (btnH + gap) * i, btnW, btnH);
                bool isHovered = btnRect.Contains(mouse.X, mouse.Y);

                if (isHovered && mouseClicked)
                {
                    upgrades[i].Action.Invoke();
                    xp.AvailableUpgradePoints--;
                    System.Console.WriteLine($"[Upgrade] Aplicado: {upgrades[i].Name} (Pontos restantes: {xp.AvailableUpgradePoints})");

                    // Se ainda há pontos, manter menu aberto; senão fechar
                    if (xp.AvailableUpgradePoints <= 0)
                    {
                        player.RemoveComponent<UpgradeRequestComponent>();
                        xp.HasPendingLevelUp = false;
                        previousMouseState = mouse;
                        return false; // menu fechado
                    }
                    
                    // Atualizar previousMouse aqui para evitar double-clicks
                    previousMouseState = mouse;
                    return true; // menu continua aberto
                }

                DrawButton(spriteBatch, pixelTexture, font, btnRect, upgrades[i].Name, isHovered);
            }
            
            // Botão "Fechar Menu" no final
            int closeY = startY + (btnH + gap) * upgrades.Length + 10;
            Rectangle closeBtnRect = new Rectangle(centerX - btnW / 2, closeY, btnW, btnH);
            bool closeHovered = closeBtnRect.Contains(mouse.X, mouse.Y);
            
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