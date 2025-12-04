using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items;
using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de pedra no mundo.
    /// </summary>
    public sealed class StoneEntityFactory
    {
        private TextureManager _textureManager;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreateStone(IGameWorld world, Vector2 position)
        {
            var entity = world.CreateEntity("Stone");
            
            entity.AddComponent(new TransformComponent(position));
            
            Texture2D texture = _textureManager?.GetTexture("stone");
            
            if (texture != null)
            {
                entity.AddComponent(new SpriteComponent(texture, 25f, 25f, null, RenderLayer.GroundItems));
            }
            else
            {
                entity.AddComponent(new SpriteComponent(Color.Gray, 25f, 25f, RenderLayer.GroundItems));
            }
            
            var item = new StoneItem();
            if (texture != null)
            {
                item.IconTexture = texture;
            }
            
            entity.AddComponent(new PickupComponent(item, quantity: 1, pickupRadius: 50f));
            
            return entity;
        }
    }

    public class StoneItem : CubeSurvivor.Inventory.Items.Item
    {
        public StoneItem() : base("stone", "Stone", "A rough stone.", CubeSurvivor.Inventory.Core.ItemType.Material, 64)
        {
        }

        private StoneItem(StoneItem original) : base(
            original.Id,
            original.Name,
            original.Description,
            original.Type,
            original.MaxStackSize,
            original.IconColor)
        {
            IconTexture = original.IconTexture;
        }

        public override CubeSurvivor.Inventory.Core.IItem Clone()
        {
            return new StoneItem(this);
        }
    }
}
