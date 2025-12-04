using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items;
using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de essência de água no mundo.
    /// </summary>
    public sealed class WaterEssenceEntityFactory
    {
        private TextureManager _textureManager;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreateWaterEssence(IGameWorld world, Vector2 position)
        {
            var entity = world.CreateEntity("WaterEssence");
            
            entity.AddComponent(new TransformComponent(position));
            
            Texture2D texture = _textureManager?.GetTexture("water_essence");
            
            if (texture != null)
            {
                entity.AddComponent(new SpriteComponent(texture, 25f, 25f, null, RenderLayer.GroundItems));
            }
            else
            {
                entity.AddComponent(new SpriteComponent(Color.Blue, 25f, 25f, RenderLayer.GroundItems));
            }
            
            var item = new WaterEssenceItem();
            if (texture != null)
            {
                item.IconTexture = texture;
            }
            
            entity.AddComponent(new PickupComponent(item, quantity: 1, pickupRadius: 50f));
            
            return entity;
        }
    }

    public class WaterEssenceItem : CubeSurvivor.Inventory.Items.Item
    {
        public WaterEssenceItem() : base("water_essence", "Water Essence", "A magical drop of water.", CubeSurvivor.Inventory.Core.ItemType.Material, 64)
        {
        }

        private WaterEssenceItem(WaterEssenceItem original) : base(
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
            return new WaterEssenceItem(this);
        }
    }
}
