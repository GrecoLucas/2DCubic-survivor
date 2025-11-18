using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace CubeSurvivor.Inventory.Systems
{
    /// <summary>
    /// Sistema responsável por gerenciar drag-and-drop de itens no inventário.
    /// Apenas funciona quando a UI do inventário está aberta.
    /// </summary>
    public sealed class InventoryDragDropSystem : GameSystem
    {
        private MouseState _previousMouseState;
        
        // Configurações de UI (devem corresponder ao InventoryUISystem)
        private const int InventorySlotSize = 50;
        private const int InventorySlotSpacing = 5;
        private const int InventoryPadding = 20;
        private const int InventorySlotsPerRow = 4;
        
        private int _screenWidth;
        private int _screenHeight;
        
        public InventoryDragDropSystem()
        {
            _previousMouseState = Mouse.GetState();
        }
        
        public void SetScreenSize(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
        }
        
        public override void Update(GameTime gameTime)
        {
            var currentMouseState = Mouse.GetState();
            
            var playerEntities = World.GetEntitiesWithComponent<PlayerInputComponent>().ToList();
            
            foreach (var entity in playerEntities)
            {
                var inventoryComp = entity.GetComponent<InventoryComponent>();
                if (inventoryComp == null || !inventoryComp.Enabled)
                    continue;
                
                // Só permitir drag-and-drop quando inventário está aberto
                if (!inventoryComp.IsUIOpen)
                {
                    _previousMouseState = currentMouseState;
                    continue;
                }
                
                var inventory = inventoryComp.Inventory;
                var dragDropComp = entity.GetComponent<DragDropComponent>();
                
                if (dragDropComp == null)
                {
                    dragDropComp = new DragDropComponent();
                    entity.AddComponent(dragDropComp);
                }
                
                ProcessDragDrop(dragDropComp, inventory, currentMouseState, _previousMouseState);
            }
            
            _previousMouseState = currentMouseState;
        }
        
        private void ProcessDragDrop(DragDropComponent dragDrop, Core.IInventory inventory, MouseState current, MouseState previous)
        {
            Vector2 mousePos = new Vector2(current.X, current.Y);
            
            if (!dragDrop.IsDragging)
            {
                // Iniciar drag se clicou em um slot com item
                if (current.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Released)
                {
                    int slotIndex = GetSlotAtPosition(mousePos, inventory.SlotCount);
                    if (slotIndex >= 0)
                    {
                        var stack = inventory.GetSlot(slotIndex);
                        if (stack != null && !stack.IsEmpty)
                        {
                            dragDrop.StartDrag(slotIndex, stack, mousePos);
                        }
                    }
                }
            }
            else
            {
                // Atualizar posição do drag
                dragDrop.UpdateDragPosition(mousePos);
                
                // Finalizar drag quando soltar o botão
                if (current.LeftButton == ButtonState.Released)
                {
                    int targetSlotIndex = GetSlotAtPosition(mousePos, inventory.SlotCount);
                    
                    if (targetSlotIndex >= 0 && targetSlotIndex != dragDrop.SourceSlotIndex)
                    {
                        // Trocar itens entre slots
                        SwapSlots(inventory, dragDrop.SourceSlotIndex, targetSlotIndex);
                    }
                    
                    dragDrop.EndDrag();
                }
            }
        }
        
        private int GetSlotAtPosition(Vector2 mousePos, int totalSlots)
        {
            int rows = (int)System.Math.Ceiling((double)totalSlots / InventorySlotsPerRow);
            int totalWidth = (InventorySlotSize * InventorySlotsPerRow) + 
                           (InventorySlotSpacing * (InventorySlotsPerRow - 1)) + 
                           (InventoryPadding * 2);
            int totalHeight = (InventorySlotSize * rows) + 
                            (InventorySlotSpacing * (rows - 1)) + 
                            (InventoryPadding * 2) + 30;
            
            int startX = (_screenWidth - totalWidth) / 2;
            int startY = (_screenHeight - totalHeight) / 2;
            int slotStartY = startY + InventoryPadding + 25;
            
            for (int i = 0; i < totalSlots; i++)
            {
                int row = i / InventorySlotsPerRow;
                int col = i % InventorySlotsPerRow;
                
                int x = startX + InventoryPadding + (col * (InventorySlotSize + InventorySlotSpacing));
                int y = slotStartY + (row * (InventorySlotSize + InventorySlotSpacing));
                
                Rectangle slotRect = new Rectangle(x, y, InventorySlotSize, InventorySlotSize);
                
                if (slotRect.Contains(mousePos))
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        private void SwapSlots(Core.IInventory inventory, int sourceIndex, int targetIndex)
        {
            var sourceStack = inventory.GetSlot(sourceIndex);
            var targetStack = inventory.GetSlot(targetIndex);
            
            // Trocar os slots
            inventory.SetSlot(sourceIndex, targetStack);
            inventory.SetSlot(targetIndex, sourceStack);
        }
    }
}

