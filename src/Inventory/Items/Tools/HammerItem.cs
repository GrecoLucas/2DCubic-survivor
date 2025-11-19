using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Tools
{
    /// <summary>
    /// Martelo que permite ao jogador construir estruturas.
    /// </summary>
    public sealed class HammerItem : Item
    {
        public HammerItem() : base(
            id: "hammer",
            name: "Hammer",
            description: "A construction tool. Hold this to build crates using wood.",
            type: ItemType.Tool,
            maxStackSize: 1,
            iconColor: new Color(169, 169, 169)) // Cinza metálico
        {
        }

        private HammerItem(HammerItem original) : base(
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
            return new HammerItem(this);
        }

        public override bool OnUse(CubeSurvivor.Core.Entity user)
        {
            // Hammer enables building; actual construction happens in ConstructionSystem
            return false;
        }
    }
}

