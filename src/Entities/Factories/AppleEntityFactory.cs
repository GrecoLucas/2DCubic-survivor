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
            
            // Usar textura se disponível, senão usar cor
            Texture2D appleTexture = _textureManager?.GetTexture("apple");
            if (appleTexture != null)
            {
                apple.AddComponent(new SpriteComponent(appleTexture, 20f, 20f));
            }
            else
            {
                apple.AddComponent(new SpriteComponent(Color.Red, 20f, 20f)); // Fallback: quadrado vermelho
            }
            
            // Componente de pickup
            var appleItem = new AppleItem();
            // Atribuir textura ao item também (para inventário)
            if (_textureManager != null)
            {
                appleItem.IconTexture = _textureManager.GetTexture("apple");
            }
            apple.AddComponent(new PickupComponent(appleItem, quantity: 1, pickupRadius: 50f));
            
            // Collider para detecção de proximidade (opcional, mas útil)
            apple.AddComponent(new ColliderComponent(20f, 20f, ColliderTag.Default));
            
            return apple;
        }
    }
}

