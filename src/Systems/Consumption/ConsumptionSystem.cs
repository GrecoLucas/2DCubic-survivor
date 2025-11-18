using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Components;
using CubeSurvivor.Inventory.Items.Consumables;
using Microsoft.Xna.Framework;
using System.Linq;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema responsável por processar o consumo de itens consumíveis.
    /// Gerencia o tempo de consumo e aplica os efeitos quando concluído.
    /// </summary>
    public sealed class ConsumptionSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            var entities = World.GetEntitiesWithComponent<ConsumptionComponent>().ToList();
            
            foreach (var entity in entities)
            {
                var consumption = entity.GetComponent<ConsumptionComponent>();
                if (consumption == null || !consumption.Enabled || !consumption.IsConsuming)
                    continue;
                
                // Incrementar progresso
                consumption.ConsumptionProgress += deltaTime;
                
                // Verificar se o consumo foi concluído
                if (consumption.ConsumptionProgress >= consumption.CurrentItem.ConsumptionTime)
                {
                    CompleteConsumption(entity, consumption);
                }
            }
        }
        
        private void CompleteConsumption(Entity entity, ConsumptionComponent consumption)
        {
            if (consumption.CurrentItem == null)
            {
                consumption.CancelConsumption();
                return;
            }
            
            // Aplicar efeito do consumível
            bool success = consumption.CurrentItem.OnConsumptionComplete(entity);
            
            if (success)
            {
                // Remover item do inventário
                var inventoryComp = entity.GetComponent<InventoryComponent>();
                if (inventoryComp != null)
                {
                    var inventory = inventoryComp.Inventory;
                    var slot = inventory.GetSlot(consumption.ItemSlotIndex);
                    
                    if (slot != null && !slot.IsEmpty && slot.Item.Id == consumption.CurrentItem.Id)
                    {
                        // Remover 1 unidade do item
                        slot.RemoveQuantity(1);
                        
                        // Se ficou vazio, limpar o slot
                        if (slot.IsEmpty)
                        {
                            inventory.SetSlot(consumption.ItemSlotIndex, null);
                        }
                        
                        // Notificar mudança no slot
                        // (o evento já é disparado automaticamente pelo inventory)
                    }
                }
            }
            
            // Limpar estado de consumo
            consumption.CompleteConsumption();
        }
    }
}

