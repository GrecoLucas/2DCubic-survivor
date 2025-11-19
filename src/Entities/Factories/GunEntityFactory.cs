using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de gun no mundo (dropável).
    /// </summary>
    public sealed class GunEntityFactory
    {
        private TextureManager _textureManager;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreateGun(IGameWorld world, Vector2 position)
        {
            var gun = world.CreateEntity("Gun");

            // Transformação e visual
            gun.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível, senão usar cor
            // Guns no chão usam a camada GroundItems para renderizar abaixo de entidades
            Texture2D gunTexture = _textureManager?.GetTexture("gun");
            if (gunTexture != null)
            {
                gun.AddComponent(new SpriteComponent(gunTexture, 25f, 6f, null, RenderLayer.GroundItems));
            }
            else
            {
                gun.AddComponent(new SpriteComponent(Color.Black, 25f, 6f, RenderLayer.GroundItems));
            }
            
            // Componente de pickup
            var gunItem = new GunItem();
            // Atribuir textura ao item também (para inventário)
            if (_textureManager != null)
            {
                gunItem.IconTexture = _textureManager.GetTexture("gun");
            }
            gun.AddComponent(new PickupComponent(gunItem, quantity: 1, pickupRadius: 50f));
            
            // Collider para detecção de proximidade
            gun.AddComponent(new ColliderComponent(25f, 6f, ColliderTag.Default));

            return gun;
        }
    }
}