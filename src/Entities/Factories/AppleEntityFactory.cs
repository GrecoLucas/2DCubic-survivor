using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Consumables;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de maçã no mundo.
    /// </summary>
    public sealed class AppleEntityFactory
    {
        public Entity CreateApple(IGameWorld world, Vector2 position)
        {
            var apple = world.CreateEntity("Apple");
            
            // Transformação e visual
            apple.AddComponent(new TransformComponent(position));
            apple.AddComponent(new SpriteComponent(Color.Red, 20f, 20f)); // Quadrado vermelho 20x20
            
            // Componente de pickup
            var appleItem = new AppleItem();
            apple.AddComponent(new PickupComponent(appleItem, quantity: 1, pickupRadius: 50f));
            
            // Collider para detecção de proximidade (opcional, mas útil)
            apple.AddComponent(new ColliderComponent(20f, 20f, ColliderTag.Default));
            
            return apple;
        }
    }
}

