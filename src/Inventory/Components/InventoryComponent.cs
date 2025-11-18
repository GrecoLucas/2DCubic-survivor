using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Core;

namespace CubeSurvivor.Inventory.Components
{
    /// <summary>
    /// Componente que armazena o inventário de uma entidade.
    /// Utiliza composição para delegar funcionalidade ao IInventory.
    /// </summary>
    public sealed class InventoryComponent : Component
    {
        public IInventory Inventory { get; }
        public bool IsUIOpen { get; set; }
        
        public InventoryComponent(IInventory inventory)
        {
            Inventory = inventory ?? throw new System.ArgumentNullException(nameof(inventory));
            IsUIOpen = false;
        }
        
        public void ToggleUI()
        {
            IsUIOpen = !IsUIOpen;
        }
    }
}

