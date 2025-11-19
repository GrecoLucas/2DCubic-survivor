using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Xml.Linq;

namespace CubeSurvivor
{
    /// <summary>
    /// Utilitário para carregar fontes com fallback automático para Liberation Sans.
    /// </summary>
    public static class FontHelper
    {
        /// <summary>
        /// Tenta carregar uma fonte. Se falhar, modifica o arquivo spritefont para usar Liberation Sans e tenta novamente.
        /// </summary>
        /// <param name="content">ContentManager do jogo</param>
        /// <param name="fontName">Nome da fonte a carregar</param>
        /// <returns>SpriteFont carregada ou null se falhar completamente</returns>
        public static SpriteFont LoadFontWithFallback(ContentManager content, string fontName)
        {
            try
            {
                // Tentar carregar a fonte normalmente
                return content.Load<SpriteFont>(fontName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FontHelper] Erro ao carregar fonte '{fontName}': {ex.Message}");
                Console.WriteLine("[FontHelper] Tentando fallback para Liberation Sans...");
                
                try
                {
                    // Tentar modificar o arquivo spritefont para usar Liberation Sans
                    if (TrySetFontFallback(content, fontName, "Liberation Sans"))
                    {
                        // Tentar carregar novamente após modificar
                        return content.Load<SpriteFont>(fontName);
                    }
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"[FontHelper] Erro no fallback: {fallbackEx.Message}");
                }
                
                return null;
            }
        }
        
        /// <summary>
        /// Tenta modificar o arquivo spritefont para usar uma fonte de fallback.
        /// Nota: Isso só funciona se o arquivo ainda não foi compilado pelo MonoGame.
        /// </summary>
        private static bool TrySetFontFallback(ContentManager content, string fontName, string fallbackFont)
        {
            try
            {
                string contentPath = content.RootDirectory;
                string spriteFontPath = Path.Combine(contentPath, $"{fontName}.spritefont");
                
                if (!File.Exists(spriteFontPath))
                {
                    Console.WriteLine($"[FontHelper] Arquivo não encontrado: {spriteFontPath}");
                    return false;
                }
                
                // Ler o arquivo XML
                XDocument doc = XDocument.Load(spriteFontPath);
                XElement fontElement = doc.Element("XnaContent")?.Element("Asset")?.Element("FontName");
                
                if (fontElement == null)
                {
                    Console.WriteLine("[FontHelper] Formato de arquivo spritefont inválido");
                    return false;
                }
                
                // Verificar se já está usando a fonte de fallback
                if (fontElement.Value == fallbackFont)
                {
                    Console.WriteLine($"[FontHelper] Arquivo já está usando '{fallbackFont}'");
                    return true;
                }
                
                // Modificar para usar a fonte de fallback
                string originalFont = fontElement.Value;
                fontElement.Value = fallbackFont;
                doc.Save(spriteFontPath);
                
                Console.WriteLine($"[FontHelper] Arquivo modificado: '{originalFont}' -> '{fallbackFont}'");
                Console.WriteLine($"[FontHelper] ⚠ Você precisa recompilar o projeto para que a mudança tenha efeito!");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FontHelper] Erro ao modificar arquivo: {ex.Message}");
                return false;
            }
        }
    }
}

