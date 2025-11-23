using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Consumables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de maçã no mundo.
    /// </summary>
    public sealed class AppleEntityFactory
    {
        private TextureManager _textureManager;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreateApple(IGameWorld world, Vector2 position)
        {
            var apple = world.CreateEntity("Apple");
            
            // Transformação e visual
            apple.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível, senão usar cor - TUDO 32x32
            // Itens no chão usam a camada GroundItems para renderizar abaixo de entidades
            Texture2D appleTexture = _textureManager?.GetTexture("apple");
            if (appleTexture != null)
            {
                apple.AddComponent(new SpriteComponent(appleTexture, 32f, 32f, null, RenderLayer.GroundItems));
            }
            else
            {
                apple.AddComponent(new SpriteComponent(Color.Red, 32f, 32f, RenderLayer.GroundItems));
            }
            
            // Componente de pickup
            var appleItem = new AppleItem();
            // Atribuir textura ao item também (para inventário)
            if (_textureManager != null)
            {
                appleItem.IconTexture = _textureManager.GetTexture("apple");
            }
            apple.AddComponent(new PickupComponent(appleItem, quantity: 1, pickupRadius: 50f));
            
            // Collider para detecção de proximidade - 32x32
            apple.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Default));
            
            return apple;
        }
    }
}

