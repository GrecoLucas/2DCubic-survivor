using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Consumables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de cérebro no mundo.
    /// </summary>
    public sealed class BrainEntityFactory
    {
        private TextureManager _textureManager;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreateBrain(IGameWorld world, Vector2 position)
        {
            var brain = world.CreateEntity("Brain");
            
            // Transformação e visual
            brain.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível, senão usar cor
            // Itens no chão usam a camada GroundItems para renderizar abaixo de entidades
            Texture2D brainTexture = _textureManager?.GetTexture("brain");
            if (brainTexture != null)
            {
                brain.AddComponent(new SpriteComponent(brainTexture, 25f, 25f, null, RenderLayer.GroundItems));
            }
            else
            {
                brain.AddComponent(new SpriteComponent(new Color(210, 180, 140), 25f, 25f, RenderLayer.GroundItems));
            }
            
            // Componente de pickup
            var brainItem = new BrainItem();
            // Atribuir textura ao item também (para inventário)
            if (_textureManager != null)
            {
                brainItem.IconTexture = _textureManager.GetTexture("brain");
            }
            brain.AddComponent(new PickupComponent(brainItem, quantity: 1, pickupRadius: 50f));
            
            // Collider para detecção
            brain.AddComponent(new ColliderComponent(25f, 25f, ColliderTag.Default));
            
            return brain;
        }
    }
}

