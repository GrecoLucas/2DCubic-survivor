using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Consumables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CubeSurvivor.Services;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de cérebro no mundo.
    /// </summary>
    public sealed class BrainEntityFactory
    {
        private readonly ITextureService _textureService;

        public BrainEntityFactory(ITextureService textureService = null)
        {
            _textureService = textureService;
        }
        
        public Entity CreateBrain(IGameWorld world, Vector2 position)
        {
            var brain = world.CreateEntity("Brain");
            
            // Transformação e visual
            brain.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível no serviço, senão usar cor sólida (bege/tan)
            var tex = _textureService?.Get("brain");
            if (tex != null)
            {
                brain.AddComponent(new SpriteComponent(tex, 64f, 64f));
            }
            else
            {
                brain.AddComponent(new SpriteComponent(new Color(210, 180, 140), 25f, 25f));
            }
            
            // Componente de pickup
            var brainItem = new BrainItem();
            brain.AddComponent(new PickupComponent(brainItem, quantity: 1, pickupRadius: 50f));
            
            // Collider para detecção
            brain.AddComponent(new ColliderComponent(25f, 25f, ColliderTag.Default));
            
            return brain;
        }
    }
}

