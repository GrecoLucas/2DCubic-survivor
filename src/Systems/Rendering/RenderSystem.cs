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

                var position = transform.Position - sprite.Size / 2;
                var rectangle = new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)sprite.Size.X,
                    (int)sprite.Size.Y
                );

                float rotation = entity.HasComponent<PlayerInputComponent>() ? 0f : transform.Rotation;

                Texture2D textureToUse = sprite.Texture ?? _pixelTexture;
                Color colorToUse = sprite.Texture != null ? sprite.TintColor : sprite.Color;

                _spriteBatch.Draw(
                    textureToUse,
                    rectangle,
                    null,
                    colorToUse,
                    rotation,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );

                var weapon = entity.GetComponent<WeaponComponent>();
                if (weapon != null && weapon.Enabled)
                {
                    float oppositeRotation = transform.Rotation + MathHelper.Pi;
                    
                    float offsetDistance = -35f;
                    Vector2 offsetDirection = new Vector2(
                        (float)System.Math.Cos(oppositeRotation),
                        (float)System.Math.Sin(oppositeRotation)
                    );
                    Vector2 weaponOffset = offsetDirection * offsetDistance;
                    
                    Vector2 weaponCenter = transform.Position + weaponOffset;
                    
                    Rectangle weaponRect = new Rectangle(
                        0,
                        0,
                        (int)weapon.Size.X,
                        (int)weapon.Size.Y
                    );

                    _spriteBatch.Draw(
                        _pixelTexture,
                        weaponCenter,
                        weaponRect,
                        weapon.Color,
                        oppositeRotation,
                        weapon.Size / 2f,
                        1f,
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            _spriteBatch.End();
        }
    }
}

