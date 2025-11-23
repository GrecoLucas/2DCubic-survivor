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
        
        /// <summary>
        /// The ID of the tool currently equipped (e.g., "hammer").
        /// Null means hands are empty and can harvest trees.
        /// </summary>
        public string EquippedToolId { get; set; }
        
        public InventoryComponent(IInventory inventory)
        {
            Inventory = inventory ?? throw new System.ArgumentNullException(nameof(inventory));
            IsUIOpen = false;
            EquippedToolId = null;
        }
        
        public void ToggleUI()
        {
            IsUIOpen = !IsUIOpen;
        }
    }
}

