using System;
using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Game.Map;
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
    /// Também remove itens das ItemLayers quando coletados.
    /// </summary>
    public sealed class PickupSystem : GameSystem
    {
        private KeyboardState _previousKeyboardState;
        private readonly ChunkedTileMap _map;
        
        public PickupSystem(ChunkedTileMap map = null)
        {
            _previousKeyboardState = Keyboard.GetState();
            _map = map;
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
                        // Se o item veio de uma ItemLayer, remover da layer também
                        var layerSource = entity.GetComponent<ItemLayerSourceComponent>();
                        if (layerSource != null && _map != null)
                        {
                            // Remove item da ItemLayer correspondente
                            _map.SetItemAtTile(layerSource.TileX, layerSource.TileY, ItemType.Empty, layerSource.LayerIndex);
                            Console.WriteLine($"[PickupSystem] Removed item from ItemLayer[{layerSource.LayerIndex}] at tile ({layerSource.TileX}, {layerSource.TileY})");
                        }
                        
                        // Marcar item para remoção do mundo
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

