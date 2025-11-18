using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema responsável por detectar e processar coleta de itens pelo jogador.
    /// O jogador pressiona E para coletar itens próximos.
    /// </summary>
    public sealed class PickupSystem : GameSystem
    {
        private KeyboardState _previousKeyboardState;
        
        public PickupSystem()
        {
            _previousKeyboardState = Keyboard.GetState();
        }
        
        public override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            
            // Verificar se E foi pressionado
            bool ePressed = currentKeyboardState.IsKeyDown(Keys.E) && _previousKeyboardState.IsKeyUp(Keys.E);
            
            if (ePressed)
            {
                ProcessPickups();
            }
            
            _previousKeyboardState = currentKeyboardState;
        }
        
        private void ProcessPickups()
        {
            // Encontrar jogador
            var players = World.GetEntitiesWithComponent<PlayerInputComponent>().ToList();
            if (players.Count == 0)
                return;
            
            var player = players[0];
            var playerTransform = player.GetComponent<TransformComponent>();
            var playerInventory = player.GetComponent<InventoryComponent>();
            
            if (playerTransform == null || playerInventory == null)
                return;
            
            // Encontrar itens próximos que podem ser coletados
            var itemsToRemove = new List<Entity>();
            
            foreach (var entity in World.GetEntitiesWithComponent<PickupComponent>())
            {
                var pickup = entity.GetComponent<PickupComponent>();
                var itemTransform = entity.GetComponent<TransformComponent>();
                
                if (pickup == null || itemTransform == null || !pickup.Enabled)
                    continue;
                
                // Verificar distância
                float distance = Vector2.Distance(playerTransform.Position, itemTransform.Position);
                
                if (distance <= pickup.PickupRadius)
                {
                    // Tentar adicionar ao inventário
                    bool success = playerInventory.Inventory.AddItem(pickup.Item, pickup.Quantity);
                    
                    if (success)
                    {
                        // Marcar item para remoção
                        itemsToRemove.Add(entity);
                    }
                }
            }
            
            // Remover itens coletados do mundo
            foreach (var item in itemsToRemove)
            {
                World.RemoveEntity(item);
            }
        }
    }
}

