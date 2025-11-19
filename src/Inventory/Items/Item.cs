using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Inventory.Items
{
    /// <summary>
    /// Classe base abstrata para todos os itens.
    /// Implementa Template Method Pattern para comportamentos comuns.
    /// </summary>
    public abstract class Item : IItem
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public ItemType Type { get; protected set; }
        public int MaxStackSize { get; protected set; }
        public Color IconColor { get; protected set; }
        public Texture2D IconTexture { get; set; }
        
        protected Item(string id, string name, string description, ItemType type, int maxStackSize = 1, Color? iconColor = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            MaxStackSize = maxStackSize;
            IconColor = iconColor ?? Color.Gray;
        }
        
        public virtual void OnEquip(CubeSurvivor.Core.Entity holder)
        {
            // Base implementation - pode ser sobrescrito
        }
        
        public virtual void OnUnequip(CubeSurvivor.Core.Entity holder)
        {
            // Base implementation - pode ser sobrescrito
        }
        
        public virtual bool OnUse(CubeSurvivor.Core.Entity user)
        {
            // Base implementation - pode ser sobrescrito
            return false;
        }
        
        public abstract IItem Clone();
    }
}

