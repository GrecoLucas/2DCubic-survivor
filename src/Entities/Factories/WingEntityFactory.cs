using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de asa de morcego no mundo.
    /// </summary>
    public sealed class WingEntityFactory
    {
        private TextureManager _textureManager;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreateWing(IGameWorld world, Vector2 position)
        {
            var wing = world.CreateEntity("Wing");
            
            // Transformação e visual
            wing.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível, senão usar cor
            // Itens no chão usam a camada GroundItems para renderizar abaixo de entidades
            Texture2D wingTexture = _textureManager?.GetTexture("wings");
            
            if (wingTexture != null)
            {
                wing.AddComponent(new SpriteComponent(wingTexture, 25f, 25f, null, RenderLayer.GroundItems));
            }
            else
            {
                wing.AddComponent(new SpriteComponent(new Color(64, 64, 96), 25f, 25f, RenderLayer.GroundItems));
            }
            
            // Componente de pickup com raio de interação
            var wingItem = new WingItem();
            // Atribuir textura ao item também (para inventário)
            // Atribuir textura ao item também (para inventário)
            if (wingTexture != null)
            {
                wingItem.IconTexture = wingTexture;
            }
            wing.AddComponent(new PickupComponent(wingItem, quantity: 1, pickupRadius: 50f));
            
            return wing;
        }
    }
}
