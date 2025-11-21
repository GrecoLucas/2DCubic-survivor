using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Resources
{
    /// <summary>
    /// Recurso de ouro usado para melhorias de armas.
    /// </summary>
    public sealed class GoldItem : Item
    {
        public GoldItem() : base(
            id: "gold",
            name: "Gold",
            description: "A precious metal used for weapon upgrades.",
            type: ItemType.Material,
            maxStackSize: 99,
            iconColor: new Color(255, 215, 0)) // Gold/yellow
        {
        }

        private GoldItem(GoldItem original) : base(
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
            return new GoldItem(this);
        }

        public override bool OnUse(CubeSurvivor.Core.Entity user)
        {
            // Gold is not directly usable; used by upgrade system
            return false;
        }
    }
}
