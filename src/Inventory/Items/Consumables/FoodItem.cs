using CubeSurvivor.Components;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Consumables
{
    /// <summary>
    /// Classe base para itens de comida.
    /// Comidas restauram saúde quando consumidas.
    /// </summary>
    public abstract class FoodItem : ConsumableItem
    {
        /// <summary>
        /// Quantidade de HP restaurada ao consumir
        /// </summary>
        public float HealthRestored { get; protected set; }
        
        protected FoodItem(
            string id,
            string name,
            string description,
            float healthRestored,
            float consumptionTime = 2f,
            int maxStackSize = 64,
            Color? iconColor = null)
            : base(id, name, description, consumptionTime, maxStackSize, iconColor)
        {
            HealthRestored = healthRestored;
        }
        
        public override bool OnConsumptionComplete(CubeSurvivor.Core.Entity consumer)
        {
            var health = consumer.GetComponent<HealthComponent>();
            if (health == null)
                return false;
            
            // Restaurar saúde, mas não ultrapassar o máximo
            float oldHealth = health.CurrentHealth;
            health.Heal(HealthRestored);
            
            return true;
        }
        
        public override void OnConsumptionStart(CubeSurvivor.Core.Entity consumer)
        {
            base.OnConsumptionStart(consumer);
            // Podemos adicionar efeitos visuais/sonoros aqui no futuro
        }
    }
}

