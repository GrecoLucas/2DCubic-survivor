using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Components;
using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace CubeSurvivor.Inventory.Systems
{
    /// <summary>
    /// Sistema responsável por renderizar a UI do inventário.
    /// Desenha hotbar sempre visível e inventário completo quando aberto.
    /// </summary>
    public sealed class InventoryUISystem : GameSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _font;
        private readonly Texture2D _pixelTexture;
        
        // Configurações visuais da hotbar
        private const int HotbarSlotSize = 50;
        private const int HotbarSlotSpacing = 5;
        private const int HotbarPadding = 10;
        private const int HotbarBorderThickness = 2;
        
        // Configurações visuais do inventário completo
        private const int InventorySlotSize = 50;
        private const int InventorySlotSpacing = 5;
        private const int InventoryPadding = 20;
        private const int InventorySlotsPerRow = 4;
        
        // Cores
        private static readonly Color SlotBackgroundColor = new Color(40, 40, 40, 200);
        private static readonly Color SlotBorderColor = new Color(100, 100, 100, 255);
        private static readonly Color SelectedSlotBorderColor = new Color(255, 255, 100, 255);
        private static readonly Color InventoryBackgroundColor = new Color(20, 20, 20, 230);
        
        private int _screenWidth;
        private int _screenHeight;
        
        public InventoryUISystem(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixelTexture)
        {
            _spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
            _font = font;
            _pixelTexture = pixelTexture ?? throw new ArgumentNullException(nameof(pixelTexture));
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
                var inventoryComp = entity.GetComponent<InventoryComponent>();
                if (inventoryComp == null || !inventoryComp.Enabled)
                    continue;
                
                var inventory = inventoryComp.Inventory;
                
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                
                // Sempre desenhar hotbar
                DrawHotbar(inventory);
                
                // Desenhar inventário completo se estiver aberto
                if (inventoryComp.IsUIOpen)
                {
                    DrawFullInventory(inventory);
                }
                
                _spriteBatch.End();
            }
        }
        
        private void DrawHotbar(IInventory inventory)
        {
            int totalWidth = (HotbarSlotSize * inventory.HotbarSize) + (HotbarSlotSpacing * (inventory.HotbarSize - 1));
            int startX = (_screenWidth - totalWidth) / 2;
            int startY = _screenHeight - HotbarSlotSize - HotbarPadding;
            
            for (int i = 0; i < inventory.HotbarSize; i++)
            {
                int x = startX + (i * (HotbarSlotSize + HotbarSlotSpacing));
                int y = startY;
                
                bool isSelected = i == inventory.SelectedHotbarIndex;
                var stack = inventory.GetSlot(i);
                
                DrawInventorySlot(x, y, HotbarSlotSize, stack, isSelected);
            }
        }
        
        private void DrawFullInventory(IInventory inventory)
        {
            int rows = (int)Math.Ceiling((double)inventory.SlotCount / InventorySlotsPerRow);
            int totalWidth = (InventorySlotSize * InventorySlotsPerRow) + 
                           (InventorySlotSpacing * (InventorySlotsPerRow - 1)) + 
                           (InventoryPadding * 2);
            int totalHeight = (InventorySlotSize * rows) + 
                            (InventorySlotSpacing * (rows - 1)) + 
                            (InventoryPadding * 2) + 30; // +30 para título
            
            int startX = (_screenWidth - totalWidth) / 2;
            int startY = (_screenHeight - totalHeight) / 2;
            
            // Background do inventário
            DrawRectangle(startX, startY, totalWidth, totalHeight, InventoryBackgroundColor);
            
            // Título
            if (_font != null)
            {
                string title = "Inventory";
                Vector2 titleSize = _font.MeasureString(title);
                Vector2 titlePos = new Vector2(
                    startX + (totalWidth - titleSize.X) / 2,
                    startY + 5
                );
                _spriteBatch.DrawString(_font, title, titlePos, Color.White);
            }
            
            // Slots
            int slotStartY = startY + InventoryPadding + 25;
            
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                int row = i / InventorySlotsPerRow;
                int col = i % InventorySlotsPerRow;
                
                int x = startX + InventoryPadding + (col * (InventorySlotSize + InventorySlotSpacing));
                int y = slotStartY + (row * (InventorySlotSize + InventorySlotSpacing));
                
                bool isSelected = i == inventory.SelectedHotbarIndex && i < inventory.HotbarSize;
                var stack = inventory.GetSlot(i);
                
                DrawInventorySlot(x, y, InventorySlotSize, stack, isSelected);
            }
        }
        
        private void DrawInventorySlot(int x, int y, int size, IItemStack stack, bool isSelected)
        {
            // Background do slot
            DrawRectangle(x, y, size, size, SlotBackgroundColor);
            
            // Borda do slot
            Color borderColor = isSelected ? SelectedSlotBorderColor : SlotBorderColor;
            int borderThickness = isSelected ? HotbarBorderThickness + 1 : HotbarBorderThickness;
            DrawRectangleBorder(x, y, size, size, borderThickness, borderColor);
            
            // Item icon e quantidade
            if (stack != null && !stack.IsEmpty)
            {
                DrawItemIcon(x, y, size, stack);
            }
        }
        
        private void DrawItemIcon(int x, int y, int size, IItemStack stack)
        {
            // Desenhar ícone do item (textura se disponível, senão cor representativa)
            int iconPadding = 8;
            int iconSize = size - (iconPadding * 2);
            
            if (stack.Item.IconTexture != null)
            {
                // Desenhar textura do item
                Rectangle iconRect = new Rectangle(
                    x + iconPadding,
                    y + iconPadding,
                    iconSize,
                    iconSize
                );
                _spriteBatch.Draw(stack.Item.IconTexture, iconRect, Color.White);
            }
            else
            {
                // Fallback: desenhar cor representativa
                DrawRectangle(
                    x + iconPadding, 
                    y + iconPadding, 
                    iconSize, 
                    iconSize, 
                    stack.Item.IconColor
                );
            }
            
            // Desenhar quantidade se > 1
            if (stack.Quantity > 1 && _font != null)
            {
                string quantityText = stack.Quantity.ToString();
                Vector2 textSize = _font.MeasureString(quantityText);
                Vector2 textPos = new Vector2(
                    x + size - textSize.X - 3,
                    y + size - textSize.Y - 2
                );
                
                // Sombra do texto
                _spriteBatch.DrawString(_font, quantityText, textPos + Vector2.One, Color.Black);
                // Texto principal
                _spriteBatch.DrawString(_font, quantityText, textPos, Color.White);
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

