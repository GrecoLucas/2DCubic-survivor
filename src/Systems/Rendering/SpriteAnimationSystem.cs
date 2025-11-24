using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using System;

namespace CubeSurvivor.Systems.Rendering
{
    /// <summary>
    /// System that updates sprite animations based on entity movement state.
    /// </summary>
    public sealed class SpriteAnimationSystem : GameSystem
    {
        private int _debugLogCounter = 0; // Limit debug spam

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _debugLogCounter++;

            foreach (var entity in World.GetEntitiesWithComponent<SpriteAnimatorComponent>())
            {
                var animator = entity.GetComponent<SpriteAnimatorComponent>();
                var velocity = entity.GetComponent<VelocityComponent>();

                if (animator == null || !animator.Enabled)
                    continue;

                // Determine if entity is moving
                bool isMoving = velocity != null && 
                               velocity.Enabled && 
                               velocity.Velocity.LengthSquared() > 0.01f;

                int oldFrame = animator.CurrentFrameIndex;

                // Update animation
                animator.Update(deltaTime, isMoving);

                // Update facing direction based on velocity
                if (velocity != null && velocity.Enabled)
                {
                    animator.UpdateFacing(velocity.Velocity);
                }

                // Debug logging (limited to avoid spam)
                if (_debugLogCounter % 60 == 0 && oldFrame != animator.CurrentFrameIndex)
                {
                    var currentFrame = animator.GetCurrentFrame();
                    string texKey = currentFrame != null ? "frame" + (animator.CurrentFrameIndex + 1) : "null";
                    Console.WriteLine($"[SpriteAnimationSystem] entity={entity.GetHashCode()} moving={isMoving} frameIndex={animator.CurrentFrameIndex} texKey={texKey}");
                }
            }
        }
    }
}

