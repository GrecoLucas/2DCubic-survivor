using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Core;

namespace CubeSurvivor.Inventory.Components
{
    /// <summary>
    /// Componente que rastreia qual item está sendo segurado pela entidade.
    /// Facilita a verificação de qual item está equipado.
    /// </summary>
    public sealed class HeldItemComponent : Component
    {
        public IItem CurrentItem { get; private set; }
        public int SlotIndex { get; private set; }
        
        public HeldItemComponent()
        {
            CurrentItem = null;
            SlotIndex = -1;
        }
        
        public void SetHeldItem(IItem item, int slotIndex)
        {
            CurrentItem = item;
            SlotIndex = slotIndex;
        }
        
        public void ClearHeldItem()
        {
            CurrentItem = null;
            SlotIndex = -1;
        }
        
        public bool IsHolding(string itemId)
        {
            return CurrentItem != null && CurrentItem.Id == itemId;
        }
        
        public bool IsHoldingItemOfType(ItemType type)
        {
            return CurrentItem != null && CurrentItem.Type == type;
        }
    }
}

