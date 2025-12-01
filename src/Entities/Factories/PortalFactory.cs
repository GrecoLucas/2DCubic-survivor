using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Entities
{
    /// <summary>
    /// Factory para criar portais de teleporte.
    /// Princípio: SRP - Responsável apenas por criar portais
    /// </summary>
    public sealed class PortalFactory
    {
        public Entity CreatePortal(
            IGameWorld world,
            Vector2 position,
            string destinationAreaId,
            PortalDirection direction,
            Vector2 relativeSpawnPosition,
            float width = 64f,
            float height = 64f)
        {
            var portal = world.CreateEntity($"Portal_to_{destinationAreaId}");
            
            // Transform
            portal.AddComponent(new TransformComponent(position));
            
            // Visual - Red portal
            portal.AddComponent(new SpriteComponent(
                new Color(255, 0, 0, 180), // Semi-transparent red
                width,
                height,
                RenderLayer.GroundEffects
            ));
            
            // Collider for detection
            portal.AddComponent(new ColliderComponent(width, height, ColliderTag.Default));
            
            // Portal data
            portal.AddComponent(new PortalComponent(
                destinationAreaId,
                direction,
                relativeSpawnPosition
            ));
            
            return portal;
        }
    }
}