using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeSurvivor.Systems
{
    /// <summary>
    /// Sistema de renderização para desenhar entidades na tela
    /// </summary>
    public class RenderSystem : GameSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private Texture2D _pixelTexture;

        public RenderSystem(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
        }

        /// <summary>
        /// Cria uma textura 1x1 pixel para desenhar formas
        /// </summary>
        public void CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public override void Update(GameTime gameTime)
        {
            // Este sistema não processa na fase Update
        }

        /// <summary>
        /// Renderiza todas as entidades com componentes visuais
        /// </summary>
        public void Draw(Matrix? cameraTransform = null)
        {
            if (_pixelTexture == null)
                return;

            var cameraTransformMatrix = cameraTransform ?? Matrix.Identity;
            _spriteBatch.Begin(transformMatrix: cameraTransformMatrix);

            foreach (var entity in World.GetEntitiesWithComponent<SpriteComponent>())
            {
                var sprite = entity.GetComponent<SpriteComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (sprite == null || transform == null || !sprite.Enabled)
                    continue;

                // Desenhar retângulo centrado na posição
                var position = transform.Position - sprite.Size / 2;
                var rectangle = new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    (int)sprite.Size.X,
                    (int)sprite.Size.Y
                );

                _spriteBatch.Draw(
                    _pixelTexture,
                    rectangle,
                    null,
                    sprite.Color,
                    transform.Rotation,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
            }

            _spriteBatch.End();
        }
    }
}
