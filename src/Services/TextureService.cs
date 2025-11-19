using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Services
{
    public interface ITextureService
    {
        Texture2D PixelTexture { get; }
        void CreatePixelTexture(GraphicsDevice graphicsDevice);
        void Register(string id, Texture2D texture);
        Texture2D Get(string id);
    }

    public class TextureService : ITextureService
    {
        private readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        public Texture2D PixelTexture { get; private set; }

        public void CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            if (PixelTexture != null)
                return;

            PixelTexture = new Texture2D(graphicsDevice, 1, 1);
            PixelTexture.SetData(new[] { Color.White });
        }

        public void Register(string id, Texture2D texture)
        {
            if (string.IsNullOrEmpty(id) || texture == null)
                return;

            _textures[id] = texture;
        }

        public Texture2D Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            _textures.TryGetValue(id, out var tex);
            return tex;
        }
    }
}
