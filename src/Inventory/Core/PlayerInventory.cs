using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeSurvivor.Inventory.Core
{
    /// <summary>
    /// Implementação do inventário do jogador.
    /// 16 slots totais: 4 na hotbar + 12 no inventário principal.
    /// </summary>
    public sealed class PlayerInventory : IInventory
    {
        private readonly IItemStack[] _slots;
        private int _selectedHotbarIndex;
        
        public int SlotCount => _slots.Length;
        public int HotbarSize { get; }
        
        public int SelectedHotbarIndex
        {
            get => _selectedHotbarIndex;
            set
            {
                if (value < 0 || value >= HotbarSize)
                    throw new ArgumentOutOfRangeException(nameof(value));
                
                if (_selectedHotbarIndex != value)
                {
                    _selectedHotbarIndex = value;
                    OnHotbarSelectionChanged?.Invoke(value);
                }
            }
        }
        
        public event Action<int> OnSlotChanged;
        public event Action<int> OnHotbarSelectionChanged;
        
        public PlayerInventory(int totalSlots = 16, int hotbarSize = 4)
        {
            if (totalSlots < hotbarSize)
                throw new ArgumentException("Total slots must be >= hotbar size");
            
            _slots = new IItemStack[totalSlots];
            HotbarSize = hotbarSize;
            _selectedHotbarIndex = 0;
        }
        
        public IItemStack GetSlot(int index)
        {
            ValidateSlotIndex(index);
            return _slots[index];
        }
        
        public bool SetSlot(int index, IItemStack stack)
        {
            ValidateSlotIndex(index);
            _slots[index] = stack;
            OnSlotChanged?.Invoke(index);
            return true;
        }
        
        public bool IsSlotEmpty(int index)
        {
            ValidateSlotIndex(index);
            return _slots[index] == null || _slots[index].IsEmpty;
        }
        
        public bool AddItem(IItem item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
                return false;
            
            int remainingQuantity = quantity;
            
            // Primeiro, tentar adicionar em stacks existentes do mesmo item
            for (int i = 0; i < SlotCount && remainingQuantity > 0; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item.Id == item.Id && !slot.IsFull)
                {
                    int amountToAdd = Math.Min(remainingQuantity, slot.Item.MaxStackSize - slot.Quantity);
                    slot.AddQuantity(amountToAdd);
                    remainingQuantity -= amountToAdd;
                    OnSlotChanged?.Invoke(i);
                }
            }
            
            // Se ainda sobrar, criar novos stacks em slots vazios
            for (int i = 0; i < SlotCount && remainingQuantity > 0; i++)
            {
                if (IsSlotEmpty(i))
                {
                    int amountToAdd = Math.Min(remainingQuantity, item.MaxStackSize);
                    _slots[i] = new ItemStack(item.Clone(), amountToAdd);
                    remainingQuantity -= amountToAdd;
                    OnSlotChanged?.Invoke(i);
                }
            }
            
            return remainingQuantity == 0;
        }
        
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0)
                return false;
            
            if (!HasItem(itemId, quantity))
                return false;
            
            int remainingToRemove = quantity;
            
            for (int i = 0; i < SlotCount && remainingToRemove > 0; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item.Id == itemId)
                {
                    int amountToRemove = Math.Min(remainingToRemove, slot.Quantity);
                    slot.RemoveQuantity(amountToRemove);
                    remainingToRemove -= amountToRemove;
                    
                    if (slot.IsEmpty)
                        _slots[i] = null;
                    
                    OnSlotChanged?.Invoke(i);
                }
            }
            
            return remainingToRemove == 0;
        }
        
        public bool HasItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0)
                return false;
            
            int totalCount = 0;
            
            foreach (var slot in _slots)
            {
                if (slot != null && !slot.IsEmpty && slot.Item.Id == itemId)
                {
                    totalCount += slot.Quantity;
                    if (totalCount >= quantity)
                        return true;
                }
            }
            
            return false;
        }
        
        public IItemStack GetSelectedHotbarItem()
        {
            return GetSlot(_selectedHotbarIndex);
        }
        
        public void SelectNextHotbarSlot()
        {
            SelectedHotbarIndex = (_selectedHotbarIndex + 1) % HotbarSize;
        }
        
        public void SelectPreviousHotbarSlot()
        {
            SelectedHotbarIndex = (_selectedHotbarIndex - 1 + HotbarSize) % HotbarSize;
        }
        
        public void SelectHotbarSlot(int index)
        {
            if (index >= 0 && index < HotbarSize)
            {
                SelectedHotbarIndex = index;
            }
        }
        
        public IEnumerable<IItemStack> GetAllStacks()
        {
            return _slots.Where(s => s != null && !s.IsEmpty);
        }
        
        public void Clear()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (!IsSlotEmpty(i))
                {
                    _slots[i] = null;
                    OnSlotChanged?.Invoke(i);
                }
            }
        }
        
        private void ValidateSlotIndex(int index)
        {
            if (index < 0 || index >= SlotCount)
                throw new ArgumentOutOfRangeException(nameof(index), $"Slot index must be between 0 and {SlotCount - 1}");
        }
    }
}

