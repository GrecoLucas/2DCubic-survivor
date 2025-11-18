using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Consumables;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de cérebro no mundo.
    /// </summary>
    public sealed class BrainEntityFactory
    {
        public Entity CreateBrain(IGameWorld world, Vector2 position)
        {
            var brain = world.CreateEntity("Brain");
            
            // Transformação e visual (bege/tan)
            brain.AddComponent(new TransformComponent(position));
            brain.AddComponent(new SpriteComponent(new Color(210, 180, 140), 25f, 25f));
            
            // Componente de pickup
            var brainItem = new BrainItem();
            brain.AddComponent(new PickupComponent(brainItem, quantity: 1, pickupRadius: 50f));
            
            // Collider para detecção
            brain.AddComponent(new ColliderComponent(25f, 25f, ColliderTag.Default));
            
            return brain;
        }
    }
}

