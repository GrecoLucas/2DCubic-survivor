using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de martelo (hammer) no mundo.
    /// </summary>
    public sealed class HammerEntityFactory
    {
        private TextureManager _textureManager;

        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }

        public Entity CreateHammer(IGameWorld world, Vector2 position)
        {
            var hammer = world.CreateEntity("Hammer");

            // Transformação e visual
            hammer.AddComponent(new TransformComponent(position));

            // Usar textura se disponível, senão usar cor - TUDO 32x32
            Texture2D hammerTexture = _textureManager?.GetTexture("hammer");
            if (hammerTexture != null)
            {
                hammer.AddComponent(new SpriteComponent(hammerTexture, 32f, 32f, null, RenderLayer.GroundItems));
            }
            else
            {
                hammer.AddComponent(new SpriteComponent(new Color(169, 169, 169), 32f, 32f, RenderLayer.GroundItems)); // Cor cinza
            }

            // Componente de pickup
            var hammerItem = new HammerItem();
            if (_textureManager != null)
            {
                hammerItem.IconTexture = _textureManager.GetTexture("hammer");
            }
            hammer.AddComponent(new PickupComponent(hammerItem, quantity: 1, pickupRadius: 50f));

            // Collider para detecção de proximidade - 32x32
            hammer.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Default));

            return hammer;
        }
    }
}

