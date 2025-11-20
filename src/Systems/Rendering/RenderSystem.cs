using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            _spriteBatch.Begin(transformMatrix: cameraTransformMatrix);

            // Ordenar entidades por camada de renderização para controlar z-order
            var sortedEntities = World.GetEntitiesWithComponent<SpriteComponent>()
                .OrderBy(e => e.GetComponent<SpriteComponent>()?.Layer ?? RenderLayer.Entities);

            foreach (var entity in sortedEntities)
            {
                var sprite = entity.GetComponent<SpriteComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (sprite == null || transform == null || !sprite.Enabled)
                    continue;

                // All entities use transform.Rotation + FacingOffsetRadians
                float rotation = transform.Rotation + sprite.FacingOffsetRadians;

                Texture2D tex = sprite.Texture ?? _pixelTexture;
                Color color = sprite.Texture != null ? sprite.TintColor : sprite.Color;

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

