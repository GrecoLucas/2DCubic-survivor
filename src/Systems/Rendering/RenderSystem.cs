using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace CubeSurvivor.Systems
{
    public class RenderSystem : GameSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _pixelTexture;

        public RenderSystem(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
        }

        public void CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public override void Update(GameTime gameTime)
        {
        }

        public void Draw(Matrix? cameraTransform = null)
        {
            if (_pixelTexture == null)
                return;

            var cameraTransformMatrix = cameraTransform ?? Matrix.Identity;
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp, // 32x32 pixel-art, no blur
                transformMatrix: cameraTransformMatrix
            );

            // Ordenar entidades por camada de renderização para controlar z-order
            var sortedEntities = World.GetEntitiesWithComponent<SpriteComponent>()
                .OrderBy(e => e.GetComponent<SpriteComponent>()?.Layer ?? RenderLayer.Entities);

            foreach (var entity in sortedEntities)
            {
                var sprite = entity.GetComponent<SpriteComponent>();
                var transform = entity.GetComponent<TransformComponent>();
                var animator = entity.GetComponent<SpriteAnimatorComponent>();

                if (sprite == null || transform == null || !sprite.Enabled)
                    continue;

                // PRIORITY: Use animator frame if available, otherwise use sprite texture
                Texture2D tex;
                string texSource = "unknown";
                if (animator != null && animator.Enabled)
                {
                    var animFrame = animator.GetCurrentFrame();
                    if (animFrame != null)
                    {
                        tex = animFrame;
                        texSource = "AnimatorFrame";
                        // Debug: log animator usage (limited to avoid spam)
                        if (System.Diagnostics.Debugger.IsAttached && (entity.GetHashCode() % 100 == 0))
                        {
                            Console.WriteLine($"[RenderSystem] entity={entity.GetHashCode()} using AnimatorFrame frameIndex={animator.CurrentFrameIndex}");
                        }
                    }
                    else
                    {
                        // Animator exists but no frame - fallback to sprite
                        tex = sprite.Texture ?? _pixelTexture;
                        texSource = sprite.Texture != null ? "SpriteTexture" : "PixelFallback";
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            Console.WriteLine($"[RenderSystem] ⚠ entity={entity.GetHashCode()} AnimatorFrame is null, using {texSource}");
                        }
                    }
                }
                else
                {
                    // No animator - use sprite texture
                    tex = sprite.Texture ?? _pixelTexture;
                    texSource = sprite.Texture != null ? "SpriteTexture" : "PixelFallback";
                }
                
                // Debug logging for missing textures (only log once per entity to avoid spam)
                if (tex == _pixelTexture)
                {
                    // Only log if we expected a texture but got pixel fallback
                    if ((animator != null && animator.Enabled && animator.GetCurrentFrame() == null && sprite.Texture == null) ||
                        (animator == null && sprite.Texture == null))
                    {
                        Console.WriteLine($"[RenderSystem] ⚠ entity={entity.GetHashCode()} using {texSource} (no texture available)");
                    }
                }

                Color color = tex != _pixelTexture ? sprite.TintColor : sprite.Color;

                // Rotation: use animator facing if available, otherwise transform rotation + facing offset
                float rotation;
                if (animator != null && animator.Enabled)
                {
                    rotation = animator.FacingDirection;
                }
                else
                {
                    rotation = transform.Rotation + sprite.FacingOffsetRadians;
                }

                // Source rectangle = full texture (no cropping)
                Rectangle? sourceRect = null;

                // Size of the source area in texture pixels
                Vector2 sourceSize = sourceRect.HasValue
                    ? new Vector2(sourceRect.Value.Width, sourceRect.Value.Height)
                    : new Vector2(tex.Width, tex.Height);

                // Origin = center of texture in texture pixels
                Vector2 origin = sourceSize / 2f;

                // Compute scale so final rendered size == sprite.Size (world units)
                Vector2 scale;

                if (sprite.Texture == null)
                {
                    // tex is _pixelTexture (1x1 pixel); scale IS the final size
                    scale = sprite.Size;
                }
                else
                {
                    // tex is a real texture; scale must be finalSize / texturePixelSize
                    scale = new Vector2(
                        sprite.Size.X / sourceSize.X,
                        sprite.Size.Y / sourceSize.Y
                    );
                }

                _spriteBatch.Draw(
                    tex,
                    transform.Position,    // entity center in world coords
                    sourceRect,
                    color,
                    rotation,
                    origin,                // center in texture pixels = rotation pivot
                    scale,                 // multiplier to achieve sprite.Size
                    SpriteEffects.None,
                    0f
                );

                // Weapon rendering removed - weapons are now entities with AttachmentComponent
            }

            _spriteBatch.End();
        }
    }
}

