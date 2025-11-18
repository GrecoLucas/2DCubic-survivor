using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Inventory.Items.Consumables
{
    /// <summary>
    /// Classe base abstrata para itens consumíveis.
    /// Define o comportamento comum de consumo com duração e efeito.
    /// </summary>
    public abstract class ConsumableItem : Item
    {
        /// <summary>
        /// Tempo necessário para consumir o item (em segundos)
        /// </summary>
        public float ConsumptionTime { get; protected set; }
        
        /// <summary>
        /// Indica se o item pode ser consumido múltiplas vezes (stackable)
        /// </summary>
        public bool IsStackable { get; protected set; }
        
        protected ConsumableItem(
            string id,
            string name,
            string description,
            float consumptionTime,
            int maxStackSize = 64,
            Color? iconColor = null)
            : base(id, name, description, ItemType.Consumable, maxStackSize, iconColor)
        {
            ConsumptionTime = consumptionTime;
            IsStackable = maxStackSize > 1;
        }
        
        /// <summary>
        /// Chamado quando o item começa a ser consumido.
        /// </summary>
        public virtual void OnConsumptionStart(CubeSurvivor.Core.Entity consumer)
        {
            // Base implementation - pode ser sobrescrito
        }
        
        /// <summary>
        /// Chamado quando o consumo é concluído com sucesso.
        /// Retorna true se o consumo foi bem-sucedido.
        /// </summary>
        public abstract bool OnConsumptionComplete(CubeSurvivor.Core.Entity consumer);
        
        /// <summary>
        /// Chamado quando o consumo é cancelado/interrompido.
        /// </summary>
        public virtual void OnConsumptionCancelled(CubeSurvivor.Core.Entity consumer)
        {
            // Base implementation - pode ser sobrescrito
        }
        
        /// <summary>
        /// Consumíveis geralmente não são equipados, mas podem ser usados.
        /// </summary>
        public override bool OnUse(CubeSurvivor.Core.Entity user)
        {
            // O uso será tratado pelo sistema de consumo
            return true;
        }
    }
}

