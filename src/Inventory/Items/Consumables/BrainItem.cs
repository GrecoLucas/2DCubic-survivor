using Microsoft.Xna.Framework;
using CubeSurvivor.Inventory.Core;
using CubeSurvivor.Components;

namespace CubeSurvivor.Inventory.Items.Consumables
{
    /// <summary>
    /// Item de cérebro - dropa de inimigos (10% chance).
    /// Restaura 100% da vida quando consumido.
    /// Leva 3 segundos para consumir.
    /// </summary>
    public sealed class BrainItem : FoodItem
    {
        private const float BrainConsumptionTime = 3f;
        private const int BrainMaxStack = 16;
        
        public BrainItem()
            : base(
                id: "brain",
                name: "Brain",
                description: "An enemy brain. Restores full HP.",
                healthRestored: 0f, // Será calculado dinamicamente
                consumptionTime: BrainConsumptionTime,
                maxStackSize: BrainMaxStack,
                iconColor: new Color(210, 180, 140)) // Bege/tan
        {
        }
        
        private BrainItem(BrainItem original)
            : base(
                original.Id,
                original.Name,
                original.Description,
                original.HealthRestored,
                original.ConsumptionTime,
                original.MaxStackSize,
                original.IconColor)
        {
        }
        
        public override bool OnConsumptionComplete(CubeSurvivor.Core.Entity consumer)
        {
            var health = consumer.GetComponent<HealthComponent>();
            if (health == null)
                return false;
            
            // Restaurar para vida máxima
            health.CurrentHealth = health.MaxHealth;
            
            return true;
        }
        
        public override IItem Clone()
        {
            return new BrainItem(this);
        }
    }
}

