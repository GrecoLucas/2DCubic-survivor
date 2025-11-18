using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace CubeSurvivor.Inventory.Systems
{
    /// <summary>
    /// Sistema responsável por processar input relacionado ao inventário.
    /// Gerencia seleção de hotbar (1-4, scroll) e toggle da UI (I).
    /// </summary>
    public sealed class InventoryInputSystem : GameSystem
    {
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        
        public InventoryInputSystem()
        {
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
        }
        
        public override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            var currentMouseState = Mouse.GetState();
            
            var playerEntities = World.GetEntitiesWithComponent<PlayerInputComponent>().ToList();
            
            foreach (var entity in playerEntities)
            {
                var inventoryComp = entity.GetComponent<InventoryComponent>();
                if (inventoryComp == null || !inventoryComp.Enabled)
                    continue;
                
                var inventory = inventoryComp.Inventory;
                var heldItemComp = entity.GetComponent<HeldItemComponent>();
                
                // Toggle inventory UI com tecla I
                if (IsKeyPressed(currentKeyboardState, _previousKeyboardState, Keys.I))
                {
                    inventoryComp.ToggleUI();
                }
                
                // Prevenir input de hotbar se a UI do inventário estiver aberta
                if (inventoryComp.IsUIOpen)
                {
                    _previousKeyboardState = currentKeyboardState;
                    _previousMouseState = currentMouseState;
                    continue;
                }
                
                // Seleção de hotbar com teclas numéricas (1-4)
                HandleNumberKeySelection(currentKeyboardState, _previousKeyboardState, inventory);
                
                // Seleção de hotbar com scroll do mouse
                HandleMouseWheelSelection(currentMouseState, _previousMouseState, inventory);
                
                // Atualizar item segurado baseado na seleção atual
                UpdateHeldItem(entity, inventory, heldItemComp);
            }
            
            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = currentMouseState;
        }
        
        private void HandleNumberKeySelection(KeyboardState current, KeyboardState previous, Core.IInventory inventory)
        {
            var numberKeys = new[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };
            
            for (int i = 0; i < numberKeys.Length && i < inventory.HotbarSize; i++)
            {
                if (IsKeyPressed(current, previous, numberKeys[i]))
                {
                    inventory.SelectHotbarSlot(i);
                    break;
                }
            }
        }
        
        private void HandleMouseWheelSelection(MouseState current, MouseState previous, Core.IInventory inventory)
        {
            int scrollDelta = current.ScrollWheelValue - previous.ScrollWheelValue;
            
            if (scrollDelta > 0)
            {
                // Scroll up - próximo slot
                inventory.SelectNextHotbarSlot();
            }
            else if (scrollDelta < 0)
            {
                // Scroll down - slot anterior
                inventory.SelectPreviousHotbarSlot();
            }
        }
        
        private void UpdateHeldItem(Entity entity, Core.IInventory inventory, HeldItemComponent heldItemComp)
        {
            if (heldItemComp == null)
                return;
            
            var selectedStack = inventory.GetSelectedHotbarItem();
            var previousItem = heldItemComp.CurrentItem;
            
            // Se mudou o item segurado
            if ((selectedStack == null || selectedStack.IsEmpty) && previousItem != null)
            {
                // Desequipar item anterior
                previousItem.OnUnequip(entity);
                heldItemComp.ClearHeldItem();
            }
            else if (selectedStack != null && !selectedStack.IsEmpty)
            {
                var newItem = selectedStack.Item;
                
                if (previousItem == null || previousItem.Id != newItem.Id)
                {
                    // Desequipar item anterior se existir
                    if (previousItem != null)
                    {
                        previousItem.OnUnequip(entity);
                    }
                    
                    // Equipar novo item
                    newItem.OnEquip(entity);
                    heldItemComp.SetHeldItem(newItem, inventory.SelectedHotbarIndex);
                }
            }
        }
        
        private bool IsKeyPressed(KeyboardState current, KeyboardState previous, Keys key)
        {
            return current.IsKeyDown(key) && previous.IsKeyUp(key);
        }
    }
}

