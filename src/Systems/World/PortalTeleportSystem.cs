using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema que gerencia teleporte entre áreas via portais.
    /// Princípio: SRP - Responsável apenas por lógica de teleporte
    /// </summary>
    public sealed class PortalTeleportSystem : GameSystem
    {
        private readonly Action<string, Vector2> _onAreaChange;
        private const float PortalCooldown = 1f; // 1 segundo de cooldown
        
        public PortalTeleportSystem(Action<string, Vector2> onAreaChange)
        {
            _onAreaChange = onAreaChange;
        }
        
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Get player
            var player = World.GetEntitiesWithComponent<PlayerInputComponent>().FirstOrDefault();
            if (player == null)
                return;
            
            var playerTransform = player.GetComponent<TransformComponent>();
            var playerCollider = player.GetComponent<ColliderComponent>();
            if (playerTransform == null || playerCollider == null)
                return;
            
            // Update portal cooldowns
            foreach (var portalEntity in World.GetEntitiesWithComponent<PortalComponent>())
            {
                var portal = portalEntity.GetComponent<PortalComponent>();
                if (portal.Cooldown > 0)
                {
                    portal.Cooldown -= deltaTime;
                }
            }
            
            // Check collision with portals
            foreach (var portalEntity in World.GetEntitiesWithComponent<PortalComponent>())
            {
                var portal = portalEntity.GetComponent<PortalComponent>();
                var portalTransform = portalEntity.GetComponent<TransformComponent>();
                var portalCollider = portalEntity.GetComponent<ColliderComponent>();
                
                if (portal == null || portalTransform == null || portalCollider == null)
                    continue;
                
                // Skip if portal on cooldown
                if (portal.Cooldown > 0)
                    continue;
                
                // Check collision
                var playerBounds = new Rectangle(
                    (int)(playerTransform.Position.X - playerCollider.Size.X / 2),
                    (int)(playerTransform.Position.Y - playerCollider.Size.Y / 2),
                    (int)playerCollider.Size.X,
                    (int)playerCollider.Size.Y
                );
                
                var portalBounds = new Rectangle(
                    (int)(portalTransform.Position.X - portalCollider.Size.X / 2),
                    (int)(portalTransform.Position.Y - portalCollider.Size.Y / 2),
                    (int)portalCollider.Size.X,
                    (int)portalCollider.Size.Y
                );
                
                if (playerBounds.Intersects(portalBounds))
                {
                    // Trigger teleport
                    Console.WriteLine($"[PortalTeleport] Player entering portal to {portal.DestinationAreaId}");
                    
                    // Set cooldown on portal
                    portal.Cooldown = PortalCooldown;
                    
                    // Notify area change (spawn position will be calculated by area manager)
                    _onAreaChange?.Invoke(portal.DestinationAreaId, portal.RelativeSpawnPosition);
                    
                    break; // Only one portal per frame
                }
            }
        }
    }
}