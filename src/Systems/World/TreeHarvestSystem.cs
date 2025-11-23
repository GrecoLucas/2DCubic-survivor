using System;
using System.Linq;
using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Game.Map;
using CubeSurvivor.Inventory.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Systems.World
{
    /// <summary>
    /// System that allows players to harvest trees with empty hands.
    /// Trees are removed from the map and drop wood pickups.
    /// </summary>
    public sealed class TreeHarvestSystem : GameSystem
    {
        private readonly BlockEntityStreamer _streamer;
        private readonly TextureManager _textureManager;
        private readonly ChunkedTileMap _map;
        private readonly Random _random = new Random();

        private KeyboardState _previousKeyboardState;

        // Harvest range in pixels
        private const float HarvestRange = 150f;

        public TreeHarvestSystem(
            BlockEntityStreamer streamer,
            TextureManager textureManager,
            ChunkedTileMap map)
        {
            _streamer = streamer ?? throw new ArgumentNullException(nameof(streamer));
            _textureManager = textureManager ?? throw new ArgumentNullException(nameof(textureManager));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            // Get player
            var player = World.GetEntitiesWithComponent<PlayerInputComponent>().FirstOrDefault();
            if (player == null)
                return;

            var playerTransform = player.GetComponent<TransformComponent>();
            var inventory = player.GetComponent<InventoryComponent>();
            
            if (playerTransform == null)
                return;

            // Check for harvest key press (E key)
            bool harvestPressed = keyboardState.IsKeyDown(Keys.E) && 
                                  _previousKeyboardState.IsKeyUp(Keys.E);

            if (harvestPressed)
            {
                // Only harvest if hands are empty (no equipped tool)
                if (inventory?.EquippedToolId == null)
                {
                    TryHarvestTree(playerTransform.Position);
                }
            }

            _previousKeyboardState = keyboardState;
        }

        private void TryHarvestTree(Vector2 playerPosition)
        {
            // Find nearby tree entities
            Entity nearestTree = null;
            float nearestDistance = HarvestRange;

            foreach (var entity in World.GetAllEntities())
            {
                // Check if entity is a tree (by name)
                if (!entity.Name.Contains("Tree"))
                    continue;

                var transform = entity.GetComponent<TransformComponent>();
                var obstacle = entity.GetComponent<ObstacleComponent>();
                
                if (transform == null || obstacle == null)
                    continue;

                // Calculate distance
                float distance = Vector2.Distance(playerPosition, transform.Position);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTree = entity;
                }
            }

            if (nearestTree != null)
            {
                HarvestTree(nearestTree);
            }
        }

        private void HarvestTree(Entity tree)
        {
            var transform = tree.GetComponent<TransformComponent>();
            if (transform == null)
                return;

            // Get tile coordinates
            Point tileCoords = _map.WorldToTileCoords(transform.Position);

            // Remove tree from map (all block layers)
            for (int layerIndex = 0; layerIndex < _map.Definition.BlockLayers.Count; layerIndex++)
            {
                if (_map.GetBlockAtTile(tileCoords.X, tileCoords.Y, layerIndex) == BlockType.Tree)
                {
                    _streamer.RemoveBlock(tileCoords.X, tileCoords.Y, layerIndex);
                    break;
                }
            }

            // Spawn wood pickups
            int woodAmount = _random.Next(2, 5); // 2-4 wood pieces
            SpawnWoodPickup(transform.Position, woodAmount);

            Console.WriteLine($"[TreeHarvest] Harvested tree at {tileCoords}, dropped {woodAmount} wood");
        }

        private void SpawnWoodPickup(Vector2 position, int amount)
        {
            var wood = World.CreateEntity("Wood");
            wood.AddComponent(new TransformComponent(position));

            var woodTexture = _textureManager.GetTexture("wood");
            if (woodTexture != null)
            {
                wood.AddComponent(new SpriteComponent(woodTexture, 32, 32, null, RenderLayer.GroundItems));
            }
            else
            {
                wood.AddComponent(new SpriteComponent(new Color(139, 90, 43), 32, 32, RenderLayer.GroundItems));
            }

            wood.AddComponent(new ColliderComponent(32, 32, ColliderTag.Pickup));

            var woodItem = new Inventory.Items.Resources.WoodItem();
            wood.AddComponent(new PickupComponent(woodItem, quantity: amount));
        }
    }
}

