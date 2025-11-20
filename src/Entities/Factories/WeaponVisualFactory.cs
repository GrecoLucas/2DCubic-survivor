using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory interface for creating weapon visual entities
    /// </summary>
    public interface IWeaponVisualFactory
    {
        Entity CreateGunVisual(IGameWorld world, Entity player, Texture2D gunTexture);
    }

    /// <summary>
    /// Factory for creating weapon visual entities that attach to player sockets
    /// </summary>
    public sealed class WeaponVisualFactory : IWeaponVisualFactory
    {
        public Entity CreateGunVisual(IGameWorld world, Entity player, Texture2D gunTexture)
        {
            var gunEnt = world.CreateEntity("GunVisual");

            var playerTransform = player.GetComponent<TransformComponent>();
            gunEnt.AddComponent(new TransformComponent(playerTransform.Position));
            
            // Gun visual: Black rectangle (texture is only for inventory/ground)
            gunEnt.AddComponent(new SpriteComponent(Color.Black, 25f, 6f, RenderLayer.Entities));
            
            // Attach to player's right hand
            gunEnt.AddComponent(new AttachmentComponent(player, AttachmentSocketId.RightHand));
            
            // Muzzle is near the tip of the gun (right side of sprite)
            gunEnt.AddComponent(new GunMuzzleComponent(new Vector2(12f, 0f)));
            
            // Start hidden - will be shown when gun is equipped
            var sprite = gunEnt.GetComponent<SpriteComponent>();
            sprite.Enabled = false;

            return gunEnt;
        }
    }
}

