namespace CubeSurvivor.Inventory.Core
{
    /// <summary>
    /// Representa uma pilha de itens (item + quantidade).
    /// </summary>
    public interface IItemStack
    {
        IItem Item { get; }
        int Quantity { get; set; }
        bool IsEmpty { get; }
        bool IsFull { get; }
        
        bool CanAddQuantity(int amount);
        void AddQuantity(int amount);
        void RemoveQuantity(int amount);
        IItemStack Split(int amount);
    }
}

