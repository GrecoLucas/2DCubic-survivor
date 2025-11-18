using System;

namespace CubeSurvivor.Inventory.Core
{
    /// <summary>
    /// Implementação concreta de uma pilha de itens.
    /// </summary>
    public sealed class ItemStack : IItemStack
    {
        public IItem Item { get; private set; }
        public int Quantity { get; set; }
        
        public bool IsEmpty => Item == null || Quantity <= 0;
        public bool IsFull => Quantity >= (Item?.MaxStackSize ?? 1);
        
        public ItemStack(IItem item, int quantity = 1)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Quantity = Math.Max(0, Math.Min(quantity, item.MaxStackSize));
        }
        
        public bool CanAddQuantity(int amount)
        {
            if (Item == null || amount <= 0) return false;
            return Quantity + amount <= Item.MaxStackSize;
        }
        
        public void AddQuantity(int amount)
        {
            if (!CanAddQuantity(amount))
                throw new InvalidOperationException("Cannot add quantity - would exceed max stack size");
            
            Quantity += amount;
        }
        
        public void RemoveQuantity(int amount)
        {
            if (amount <= 0 || amount > Quantity)
                throw new InvalidOperationException("Invalid quantity to remove");
            
            Quantity -= amount;
        }
        
        public IItemStack Split(int amount)
        {
            if (amount <= 0 || amount > Quantity)
                throw new InvalidOperationException("Invalid split amount");
            
            Quantity -= amount;
            return new ItemStack(Item.Clone(), amount);
        }
    }
}

