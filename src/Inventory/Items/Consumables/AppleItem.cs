using Microsoft.Xna.Framework;
using CubeSurvivor.Inventory.Core;

namespace CubeSurvivor.Inventory.Items.Consumables
{
    /// <summary>
    /// Item de maçã - restaura 5 HP quando consumida.
    /// Pode ser empilhada até 64 unidades.
    /// Leva 2 segundos para consumir.
    /// </summary>
    public sealed class AppleItem : FoodItem
    {
        private const float AppleHealthRestore = 5f;
        private const float AppleConsumptionTime = 2f;
        private const int AppleMaxStack = 64;
        
        public AppleItem()
            : base(
                id: "apple",
                name: "Apple",
                description: "A fresh apple. Restores 5 HP.",
                healthRestored: AppleHealthRestore,
                consumptionTime: AppleConsumptionTime,
                maxStackSize: AppleMaxStack,
                iconColor: Color.Red)
        {
        }
        
        private AppleItem(AppleItem original)
            : base(
                original.Id,
                original.Name,
                original.Description,
                original.HealthRestored,
                original.ConsumptionTime,
                original.MaxStackSize,
                original.IconColor)
        {
            // Copiar textura do original
            IconTexture = original.IconTexture;
        }
        
        public override IItem Clone()
        {
            var cloned = new AppleItem(this);
            return cloned;
        }
    }
}

