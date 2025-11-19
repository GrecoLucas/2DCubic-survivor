using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Resources
{
    /// <summary>
    /// Recurso de madeira usado para construção.
    /// </summary>
    public sealed class WoodItem : Item
    {
        public WoodItem() : base(
            id: "wood",
            name: "Wood",
            description: "A wooden resource used for construction. Can be used to build crates.",
            type: ItemType.Material,
            maxStackSize: 99,
            iconColor: new Color(139, 90, 43)) // Marrom madeira
        {
        }

        private WoodItem(WoodItem original) : base(
            original.Id,
            original.Name,
            original.Description,
            original.Type,
            original.MaxStackSize,
            original.IconColor)
        {
            IconTexture = original.IconTexture;
        }

        public override IItem Clone()
        {
            return new WoodItem(this);
        }

        public override bool OnUse(CubeSurvivor.Core.Entity user)
        {
            // Wood is not directly usable; it's consumed by construction system
            return false;
        }
    }
}

