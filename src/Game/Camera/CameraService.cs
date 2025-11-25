using CubeSurvivor.Components;
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor
{
    /// <summary>
    /// Serviço de câmera para centralizar a lógica de câmera.
    /// Segue o jogador e mantém a câmera centralizada nele.
    /// </summary>
    public sealed class CameraService
    {
        public Matrix Transform { get; private set; } = Matrix.Identity;
        public int ScreenWidth => _screenWidth;
        public int ScreenHeight => _screenHeight;

        private int _screenWidth;
        private int _screenHeight;
        private int _mapWidth;
        private int _mapHeight;

        public CameraService(int screenWidth, int screenHeight, int mapWidth, int mapHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
        }

        /// <summary>
        /// Atualiza as dimensões do mapa (útil quando carregado dinamicamente).
        /// </summary>
        public void SetMapSize(int mapWidth, int mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
        }

        /// <summary>
        /// Atualiza as dimensões do viewport (útil quando a janela é redimensionada).
        /// </summary>
        public void SetViewportSize(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public void Update(Entity player)
        {
            var transform = player.GetComponent<TransformComponent>();
            var sprite = player.GetComponent<SpriteComponent>();
            if (transform == null || sprite == null)
                return;

            var half = sprite.Size / 2f;

            // Clamp player inside map
            transform.Position = new Vector2(
                MathHelper.Clamp(transform.Position.X, half.X, _mapWidth - half.X),
                MathHelper.Clamp(transform.Position.Y, half.Y, _mapHeight - half.Y)
            );

            float camX = transform.Position.X;
            float camY = transform.Position.Y;

            camX = MathHelper.Clamp(camX, _screenWidth / 2f, _mapWidth - _screenWidth / 2f);
            camY = MathHelper.Clamp(camY, _screenHeight / 2f, _mapHeight - _screenHeight / 2f);

            Transform = Matrix.CreateTranslation(
                -camX + _screenWidth / 2f,
                -camY + _screenHeight / 2f,
                0f
            );
        }
    }
}

