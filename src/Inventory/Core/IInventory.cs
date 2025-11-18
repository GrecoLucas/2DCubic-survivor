using System;
using System.Collections.Generic;

namespace CubeSurvivor.Inventory.Core
{
    /// <summary>
    /// Interface para sistema de inventário.
    /// Permite diferentes implementações (player, chest, etc).
    /// </summary>
    public interface IInventory
    {
        int SlotCount { get; }
        int HotbarSize { get; }
        int SelectedHotbarIndex { get; set; }
        
        IItemStack GetSlot(int index);
        bool SetSlot(int index, IItemStack stack);
        bool IsSlotEmpty(int index);
        
        bool AddItem(IItem item, int quantity = 1);
        bool RemoveItem(string itemId, int quantity = 1);
        bool HasItem(string itemId, int quantity = 1);
        
        IItemStack GetSelectedHotbarItem();
        void SelectNextHotbarSlot();
        void SelectPreviousHotbarSlot();
        void SelectHotbarSlot(int index);
        
        IEnumerable<IItemStack> GetAllStacks();
        void Clear();
        
        event Action<int> OnSlotChanged;
        event Action<int> OnHotbarSelectionChanged;
    }
}

