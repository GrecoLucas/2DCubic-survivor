using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CubeSurvivor
{
    /// <summary>
    /// Renderizador eficiente do background do mundo usando camera culling.
    /// Apenas desenha os tiles visíveis na tela, permitindo mapas muito grandes sem perda de performance.
    /// </summary>
    public sealed class WorldBackgroundRenderer
    {
        // Escalonável: nenhum conceito fixo de "floor" ou metades; tudo vem do provider de bioma.
        // Se quiser pré-computar no futuro, pode-se armazenar uma grade de texturas derivada dos biomas.
        private System.Func<Microsoft.Xna.Framework.Vector2, Texture2D> _biomeTextureProvider;
        private int _mapWidth;
        private int _mapHeight;
        private readonly int _tileSize;
        private readonly int _screenWidth;
        private readonly int _screenHeight;

        public WorldBackgroundRenderer(int mapWidth, int mapHeight, int tileSize, int screenWidth, int screenHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _tileSize = tileSize;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        // Configura provider que retorna textura do bioma para uma posição; obrigatório para renderização.
        public void SetBiomeTextureProvider(System.Func<Microsoft.Xna.Framework.Vector2, Texture2D> provider)
        {
            _biomeTextureProvider = provider;
        }

        /// <summary>
        /// Atualiza as dimensões do mapa (útil quando carregado dinamicamente).
        /// </summary>
        public void SetMapSize(int mapWidth, int mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            System.Console.WriteLine($"[BackgroundRenderer] Dimensões atualizadas para {mapWidth}x{mapHeight}");
        }

        /// <summary>
        /// Desenha apenas os tiles do background que estão visíveis pela câmera.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch para desenhar</param>
        /// <param name="cameraTransform">Transformação da câmera</param>
        public void Draw(SpriteBatch spriteBatch, Matrix cameraTransform)
        {
            // Sem provider não há nada para desenhar (tudo é data-driven via JSON -> BiomeSystem).
            if (_biomeTextureProvider == null)
                return;

            // Calcular o retângulo visível no mundo
            Matrix inverse = Matrix.Invert(cameraTransform);

            Vector2 topLeft = Vector2.Transform(Vector2.Zero, inverse);
            Vector2 bottomRight = Vector2.Transform(new Vector2(_screenWidth, _screenHeight), inverse);

            // Clampear aos limites do mapa
            float left = MathF.Max(0, topLeft.X);
            float top = MathF.Max(0, topLeft.Y);
            float right = MathF.Min(_mapWidth, bottomRight.X);
            float bottom = MathF.Min(_mapHeight, bottomRight.Y);

            // Calcular quais tiles precisam ser desenhados
            int minTileX = (int)MathF.Floor(left / _tileSize);
            int maxTileX = (int)MathF.Ceiling(right / _tileSize);
            int minTileY = (int)MathF.Floor(top / _tileSize);
            int maxTileY = (int)MathF.Ceiling(bottom / _tileSize);

            spriteBatch.Begin(transformMatrix: cameraTransform);

            // Desenhar apenas os tiles visíveis
            for (int y = minTileY; y <= maxTileY; y++)
            {
                for (int x = minTileX; x <= maxTileX; x++)
                {
                    int worldX = x * _tileSize;
                    int worldY = y * _tileSize;

                    // Verificar se está dentro dos limites do mapa
                    if (worldX >= _mapWidth || worldY >= _mapHeight)
                        continue;

                    var tileCenter = new Microsoft.Xna.Framework.Vector2(worldX + _tileSize / 2, worldY + _tileSize / 2);
                    var tex = _biomeTextureProvider(tileCenter);
                    if (tex == null)
                        continue; // nenhum bioma cobre este tile -> não desenha
                    var dest = new Rectangle(worldX, worldY, _tileSize, _tileSize);
                    spriteBatch.Draw(tex, dest, Color.White);
                }
            }

            spriteBatch.End();
        }
    }
}

