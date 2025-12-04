using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Items;
using CubeSurvivor.Inventory.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar entidades de relíquia de chefe no mundo.
    /// </summary>
    public sealed class BossRelicEntityFactory
    {
        private TextureManager _textureManager;
        
        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }
        
        public Entity CreateBossRelic(IGameWorld world, Vector2 position)
        {
            var relic = world.CreateEntity("BossRelic");
            
            // Transformação e visual
            relic.AddComponent(new TransformComponent(position));
            
            // Usar textura se disponível, senão usar cor
            Texture2D texture = _textureManager?.GetTexture("boss_relic");
            
            if (texture != null)
            {
                relic.AddComponent(new SpriteComponent(texture, 32f, 32f, null, RenderLayer.GroundItems));
            }
            else
            {
                relic.AddComponent(new SpriteComponent(Color.Purple, 32f, 32f, RenderLayer.GroundItems));
            }
            
            // Componente de pickup
            // Note: BossRelicItem needs to be implemented or we use a generic ResourceItem if it exists, 
            // but for now I'll assume we might need a specific item class or just use a placeholder.
            // Checking existing items... assuming we can create a generic item or I need to create BossRelicItem class too.
            // For now, I will create a simple Item implementation inline or assume it exists. 
            // Actually, looking at WingEntityFactory, it uses WingItem. I should probably create BossRelicItem too if it doesn't exist.
            // But to keep it simple and strictly follow the plan, I will create the Item class here or in a separate file if needed.
            // Let's check if I can just use a generic ResourceItem.
            // Since I cannot see all files, I will create a simple internal class or just use ResourceItem if I knew it existed.
            // I'll create a BossRelicItem class in the same file for now or separate if I could.
            // Better yet, I'll create a new file for the Item if I have to, but the prompt didn't explicitly ask for Item classes, just factories.
            // However, the factory needs to instantiate an Item.
            // I'll assume I can create a simple Item instance.
            
            var item = new BossRelicItem();
            if (texture != null)
            {
                item.IconTexture = texture;
            }
            
            relic.AddComponent(new PickupComponent(item, quantity: 1, pickupRadius: 50f));
            
            return relic;
        }
    }

    public class BossRelicItem : CubeSurvivor.Inventory.Items.Item
    {
        public BossRelicItem() : base("boss_relic", "Boss Relic", "A mystical ancient relic.", CubeSurvivor.Inventory.Core.ItemType.Quest, 1)
        {
        }

        private BossRelicItem(BossRelicItem original) : base(
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
            return new BossRelicItem(this);
        }
    }
}
