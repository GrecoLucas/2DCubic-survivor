using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Consumables;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente que rastreia o estado de consumo de um item.
    /// </summary>
    public sealed class ConsumptionComponent : Component
    {
        public bool IsConsuming { get; set; }
        public float ConsumptionProgress { get; set; }
        public ConsumableItem CurrentItem { get; set; }
        public int ItemSlotIndex { get; set; }
        
        public ConsumptionComponent()
        {
            IsConsuming = false;
            ConsumptionProgress = 0f;
            CurrentItem = null;
            ItemSlotIndex = -1;
        }
        
        public void StartConsumption(ConsumableItem item, int slotIndex)
        {
            IsConsuming = true;
            ConsumptionProgress = 0f;
            CurrentItem = item;
            ItemSlotIndex = slotIndex;
        }
        
        public void CancelConsumption()
        {
            IsConsuming = false;
            ConsumptionProgress = 0f;
            CurrentItem = null;
            ItemSlotIndex = -1;
        }
        
        public void CompleteConsumption()
        {
            IsConsuming = false;
            ConsumptionProgress = 0f;
            CurrentItem = null;
            ItemSlotIndex = -1;
        }
    }
}

