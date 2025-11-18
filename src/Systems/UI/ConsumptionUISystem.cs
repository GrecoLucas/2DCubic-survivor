using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema responsável por renderizar a UI de consumo (countdown timer).
    /// Aparece no canto superior direito apenas durante o consumo.
    /// </summary>
    public sealed class ConsumptionUISystem : GameSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixelTexture;
        
        private const int UIMargin = 20;
        private const int BarWidth = 200;
        private const int BarHeight = 30;
        private const int BorderThickness = 2;
        
        private static readonly Color BackgroundColor = new Color(20, 20, 20, 200);
        private static readonly Color BorderColor = new Color(100, 100, 100, 255);
        private static readonly Color ProgressColor = new Color(100, 200, 100, 255);
        
        private int _screenWidth;
        private int _screenHeight;
        
        public ConsumptionUISystem(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            _spriteBatch = spriteBatch;
            _font = font;
            _pixelTexture = pixelTexture;
        }
        
        public void SetScreenSize(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
        }
        
        public override void Update(GameTime gameTime)
        {
            // Sistema de renderização - não precisa de Update
        }
        
        public void Draw()
        {
            if (_pixelTexture == null)
                return;
            
            var playerEntities = World.GetEntitiesWithComponent<PlayerInputComponent>().ToList();
            
            foreach (var entity in playerEntities)
            {
                var consumption = entity.GetComponent<ConsumptionComponent>();
                if (consumption == null || !consumption.Enabled || !consumption.IsConsuming)
                    continue;
                
                var currentItem = consumption.CurrentItem;
                if (currentItem == null)
                    continue;
                
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                
                DrawConsumptionBar(consumption);
                
                _spriteBatch.End();
            }
        }
        
        private void DrawConsumptionBar(ConsumptionComponent consumption)
        {
            // Posição no canto superior direito
            int x = _screenWidth - BarWidth - UIMargin;
            int y = UIMargin;
            
            // Background
            DrawRectangle(x, y, BarWidth, BarHeight, BackgroundColor);
            
            // Borda
            DrawRectangleBorder(x, y, BarWidth, BarHeight, BorderThickness, BorderColor);
            
            // Barra de progresso
            float progress = consumption.ConsumptionProgress / consumption.CurrentItem.ConsumptionTime;
            progress = MathHelper.Clamp(progress, 0f, 1f);
            int progressWidth = (int)(BarWidth * progress);
            
            if (progressWidth > 0)
            {
                DrawRectangle(x, y, progressWidth, BarHeight, ProgressColor);
            }
            
            // Texto (nome do item e tempo restante)
            if (_font != null)
            {
                string itemName = consumption.CurrentItem.Name;
                float remainingTime = consumption.CurrentItem.ConsumptionTime - consumption.ConsumptionProgress;
                string timeText = $"{remainingTime:F1}s";
                
                // Desenhar nome do item centralizado
                Vector2 nameSize = _font.MeasureString(itemName);
                Vector2 namePos = new Vector2(
                    x + (BarWidth - nameSize.X) / 2,
                    y + 3
                );
                
                _spriteBatch.DrawString(_font, itemName, namePos + Vector2.One, Color.Black);
                _spriteBatch.DrawString(_font, itemName, namePos, Color.White);
                
                // Desenhar tempo no canto direito
                Vector2 timeSize = _font.MeasureString(timeText);
                Vector2 timePos = new Vector2(
                    x + BarWidth - timeSize.X - 5,
                    y + BarHeight - timeSize.Y - 3
                );
                
                _spriteBatch.DrawString(_font, timeText, timePos + Vector2.One, Color.Black);
                _spriteBatch.DrawString(_font, timeText, timePos, Color.White);
            }
        }
        
        private void DrawRectangle(int x, int y, int width, int height, Color color)
        {
            _spriteBatch.Draw(
                _pixelTexture,
                new Rectangle(x, y, width, height),
                color
            );
        }
        
        private void DrawRectangleBorder(int x, int y, int width, int height, int thickness, Color color)
        {
            // Top
            DrawRectangle(x, y, width, thickness, color);
            // Bottom
            DrawRectangle(x, y + height - thickness, width, thickness, color);
            // Left
            DrawRectangle(x, y, thickness, height, color);
            // Right
            DrawRectangle(x + width - thickness, y, thickness, height, color);
        }
    }
}

