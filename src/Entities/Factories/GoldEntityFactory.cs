using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de ouro (gold) no mundo.
    /// </summary>
    public sealed class GoldEntityFactory
    {
        private TextureManager _textureManager;

        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }

        public Entity CreateGold(IGameWorld world, Vector2 position, int quantity = 1)
        {
            var gold = world.CreateEntity("Gold");

            gold.AddComponent(new TransformComponent(position));

            // TUDO 32x32
            Texture2D goldTexture = _textureManager?.GetTexture("gold");
            if (goldTexture != null)
            {
                gold.AddComponent(new SpriteComponent(goldTexture, 32f, 32f, null, RenderLayer.GroundItems));
            }
            else
            {
                gold.AddComponent(new SpriteComponent(new Color(255, 215, 0), 32f, 32f, RenderLayer.GroundItems)); // Gold color
            }

            var goldItem = new GoldItem();
            if (_textureManager != null)
            {
                goldItem.IconTexture = _textureManager.GetTexture("gold");
            }
            gold.AddComponent(new PickupComponent(goldItem, quantity: quantity, pickupRadius: 50f));

            gold.AddComponent(new ColliderComponent(32f, 32f, ColliderTag.Default));

            return gold;
        }
    }
}
