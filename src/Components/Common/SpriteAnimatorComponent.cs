using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Component for sprite animation (walk cycles, idle states, etc.).
    /// Manages frame timing and selection based on movement state.
    /// </summary>
    public class SpriteAnimatorComponent : Component
    {
        /// <summary>
        /// Array of animation frames (textures).
        /// </summary>
        public Texture2D[] Frames { get; set; }

        /// <summary>
        /// Animation speed in frames per second.
        /// </summary>
        public float Fps { get; set; } = 10f;

        /// <summary>
        /// Current frame index (0-based).
        /// </summary>
        public int CurrentFrameIndex { get; set; } = 0;

        /// <summary>
        /// Accumulated time since last frame change.
        /// </summary>
        public float FrameTime { get; set; } = 0f;

        /// <summary>
        /// Whether the animation is currently playing (entity is moving).
        /// </summary>
        public bool IsPlaying { get; set; } = false;

        /// <summary>
        /// Current facing direction in radians (0 = right, PI/2 = down, PI = left, 3*PI/2 = up).
        /// </summary>
        public float FacingDirection { get; set; } = 0f;

        /// <summary>
        /// Whether to loop the animation when playing.
        /// </summary>
        public bool Loop { get; set; } = true;

        public SpriteAnimatorComponent(Texture2D[] frames, float fps = 10f)
        {
            Frames = frames ?? throw new ArgumentNullException(nameof(frames));
            Fps = fps;
            CurrentFrameIndex = 0;
            FrameTime = 0f;
            IsPlaying = false;
            Loop = true;
        }

        /// <summary>
        /// Updates animation state based on elapsed time.
        /// </summary>
        public void Update(float deltaTime, bool isMoving)
        {
            IsPlaying = isMoving;

            if (IsPlaying && Frames != null && Frames.Length > 1)
            {
                FrameTime += deltaTime;
                float frameDuration = 1f / Fps;

                if (FrameTime >= frameDuration)
                {
                    FrameTime -= frameDuration;
                    CurrentFrameIndex++;

                    if (CurrentFrameIndex >= Frames.Length)
                    {
                        if (Loop)
                        {
                            CurrentFrameIndex = 0;
                        }
                        else
                        {
                            CurrentFrameIndex = Frames.Length - 1; // Stay on last frame
                        }
                    }
                }
            }
            else
            {
                // Idle: stay on first frame
                CurrentFrameIndex = 0;
                FrameTime = 0f;
            }
        }

        /// <summary>
        /// Gets the current frame texture, or null if no frames.
        /// </summary>
        public Texture2D GetCurrentFrame()
        {
            if (Frames == null || Frames.Length == 0)
                return null;

            int index = Math.Clamp(CurrentFrameIndex, 0, Frames.Length - 1);
            return Frames[index];
        }

        /// <summary>
        /// Updates facing direction based on velocity vector.
        /// </summary>
        public void UpdateFacing(Vector2 velocity)
        {
            if (velocity.LengthSquared() < 0.01f)
                return; // Too small to determine direction

            // Calculate angle in radians
            // Atan2 returns: 0 = right, PI/2 = down, PI = left, -PI/2 = up
            float angle = (float)Math.Atan2(velocity.Y, velocity.X);
            FacingDirection = angle;
        }
    }
}

