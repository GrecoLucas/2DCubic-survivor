using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace CubeSurvivor.Core
{
    /// <summary>
    /// Gerenciador centralizado para carregar e armazenar texturas do jogo.
    /// </summary>
    public sealed class TextureManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, Texture2D> _textures;
        
        public TextureManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _textures = new Dictionary<string, Texture2D>();
        }
        
        /// <summary>
        /// Carrega uma textura a partir de um caminho de arquivo.
        /// Se a textura já foi carregada, retorna a cópia em cache.
        /// </summary>
        public Texture2D LoadTexture(string key, string filePath)
        {
            if (_textures.ContainsKey(key))
            {
                return _textures[key];
            }
            
            try
            {
                // Tentar vários caminhos possíveis
                string[] candidates = new[]
                {
                    filePath,
                    Path.Combine("assets", filePath),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", filePath),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "assets", filePath)
                };
                
                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                    {
                        Console.WriteLine($"[TextureManager] Carregando textura '{key}' de: {path}");
                        using (var stream = File.OpenRead(path))
                        {
                            var texture = Texture2D.FromStream(_graphicsDevice, stream);
                            _textures[key] = texture;
                            return texture;
                        }
                    }
                }
                
                Console.WriteLine($"[TextureManager] ⚠ Textura '{key}' não encontrada em: {filePath}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextureManager] ⚠ Erro ao carregar textura '{key}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Obtém uma textura já carregada.
        /// </summary>
        public Texture2D GetTexture(string key)
        {
            return _textures.TryGetValue(key, out var texture) ? texture : null;
        }
        
        /// <summary>
        /// Verifica se uma textura foi carregada.
        /// </summary>
        public bool HasTexture(string key)
        {
            return _textures.ContainsKey(key);
        }
        
        /// <summary>
        /// Remove uma textura do cache e libera seus recursos.
        /// </summary>
        public void UnloadTexture(string key)
        {
            if (_textures.TryGetValue(key, out var texture))
            {
                texture?.Dispose();
                _textures.Remove(key);
            }
        }
        
        /// <summary>
        /// Libera todos os recursos de textura.
        /// </summary>
        public void UnloadAll()
        {
            foreach (var texture in _textures.Values)
            {
                texture?.Dispose();
            }
            _textures.Clear();
        }
    }
}

