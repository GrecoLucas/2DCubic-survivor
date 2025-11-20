using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Inventory.Components;
using CubeSurvivor.Inventory.Items.Weapons;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que controla a visibilidade dos visuals de armas baseado no item equipado
    /// </summary>
    public sealed class WeaponVisualSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            // Para cada player com inventário
            foreach (var player in World.GetEntitiesWithComponent<HeldItemComponent>())
            {
                var heldItem = player.GetComponent<HeldItemComponent>();
                if (heldItem == null)
                    continue;

                // Verificar se tem gun equipada
                bool hasGunEquipped = heldItem.CurrentItem is GunItem;

                // Encontrar o visual da gun attachado a este player
                foreach (var visual in World.GetEntitiesWithComponent<GunMuzzleComponent>())
                {
                    var attachment = visual.GetComponent<AttachmentComponent>();
                    if (attachment != null && attachment.Parent == player)
                    {
                        var sprite = visual.GetComponent<SpriteComponent>();
                        if (sprite != null)
                        {
                            // Mostrar visual apenas se gun está equipada
                            sprite.Enabled = hasGunEquipped;
                        }
                    }
                }
            }
        }
    }
}

