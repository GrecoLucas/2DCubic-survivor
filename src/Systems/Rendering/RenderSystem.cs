using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Services;
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
        private readonly ITextureService _textureService;

        public RenderSystem(SpriteBatch spriteBatch, ITextureService textureService)
        {
            _spriteBatch = spriteBatch;
            _textureService = textureService;
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
            var pixel = _textureService?.PixelTexture;
            if (pixel == null)
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

                // Não aplicar rotação ao player (entidades com PlayerInputComponent)
                float rotation = entity.HasComponent<PlayerInputComponent>() ? 0f : transform.Rotation;

                // Usar textura se disponível, senão usar pixel texture
                Texture2D textureToUse = sprite.Texture ?? pixel;
                
                _spriteBatch.Draw(
                    textureToUse,
                    rectangle,
                    null,
                    sprite.Color,
                    rotation,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );

                // Desenhar armas (WeaponComponent)
                var weapon = entity.GetComponent<WeaponComponent>();
                if (weapon != null && weapon.Enabled)
                {
                    // Calcular direção oposta ao mouse (lado oposto do raio)
                    float oppositeRotation = transform.Rotation + MathHelper.Pi;
                    
                    // Calcular offset na direção oposta ao mouse
                    // Usar offsetDistance do componente ou padrão de 0 para centro exato
                    float offsetDistance = -35f;
                    Vector2 offsetDirection = new Vector2(
                        (float)System.Math.Cos(oppositeRotation),
                        (float)System.Math.Sin(oppositeRotation)
                    );
                    Vector2 weaponOffset = offsetDirection * offsetDistance;
                    
                    // Posição do centro da arma: centro do player + offset na direção oposta
                    Vector2 weaponCenter = transform.Position + weaponOffset;
                    
                    // Criar retângulo para a arma (tamanho)
                    Rectangle weaponRect = new Rectangle(
                        0,
                        0,
                        (int)weapon.Size.X,
                        (int)weapon.Size.Y
                    );

                    // Desenhar arma rotacionada na direção oposta ao mouse
                    // Usar weaponCenter como posição (Vector2) e origem no centro para rotação correta
                    _spriteBatch.Draw(
                        pixel,
                        weaponCenter, // Posição do centro da arma (Vector2)
                        weaponRect, // Retângulo de origem (tamanho)
                        weapon.Color,
                        oppositeRotation, // Rotação oposta ao mouse
                        weapon.Size / 2f, // Origem no centro para rotação correta
                        1f, // Escala
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            _spriteBatch.End();
        }
    }
}
