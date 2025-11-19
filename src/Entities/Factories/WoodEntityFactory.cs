using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de madeira (wood) no mundo.
    /// </summary>
    public sealed class WoodEntityFactory
    {
        private TextureManager _textureManager;

        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }

        public Entity CreateWood(IGameWorld world, Vector2 position, int quantity = 1)
        {
            var wood = world.CreateEntity("Wood");

            // Transformação e visual
            wood.AddComponent(new TransformComponent(position));

            // Usar textura se disponível, senão usar cor
            Texture2D woodTexture = _textureManager?.GetTexture("wood");
            if (woodTexture != null)
            {
                wood.AddComponent(new SpriteComponent(woodTexture, 18f, 18f, null, RenderLayer.GroundItems));
            }
            else
            {
                wood.AddComponent(new SpriteComponent(new Color(139, 90, 43), 18f, 18f, RenderLayer.GroundItems)); // Cor marrom
            }

            // Componente de pickup
            var woodItem = new WoodItem();
            if (_textureManager != null)
            {
                woodItem.IconTexture = _textureManager.GetTexture("wood");
            }
            wood.AddComponent(new PickupComponent(woodItem, quantity: quantity, pickupRadius: 50f));

            // Collider para detecção de proximidade
            wood.AddComponent(new ColliderComponent(18f, 18f, ColliderTag.Default));

            return wood;
        }
    }
}

