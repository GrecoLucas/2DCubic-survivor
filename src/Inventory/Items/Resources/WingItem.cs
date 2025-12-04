using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Resources
{
    /// <summary>
    /// Recurso de asa de morcego usado para crafting.
    /// Dropado por morcegos (10% de chance).
    /// </summary>
    public sealed class WingItem : Item
    {
        public WingItem() : base(
            id: "wing",
            name: "Bat Wing",
            description: "A wing dropped by bats. Can be used for crafting.",
            type: ItemType.Material,
            maxStackSize: 99,
            iconColor: new Color(64, 64, 96)) // Cor escura azulada
        {
        }

        private WingItem(WingItem original) : base(
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
            return new WingItem(this);
        }

        public override bool OnUse(CubeSurvivor.Core.Entity user)
        {
            // Wing is not directly usable; it's consumed by crafting system
            return false;
        }
    }
}
