using System;
using System.IO;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Carrega definições de mapas de arquivos JSON.
    /// Usa WorldDefinitionLoader existente para manter compatibilidade.
    /// </summary>
    public static class MapLoader
    {
        /// <summary>
        /// Carrega um mapa de arquivo JSON
        /// </summary>
        public static MapDefinition Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[MapLoader] File not found: {path}");
                    return null;
                }
                
                var levelDef = WorldDefinitionLoader.LoadFromJson(path);
                if (levelDef == null)
                {
                    Console.WriteLine($"[MapLoader] Failed to load level from {path}");
                    return null;
                }
                
                return MapDefinition.FromLevelDefinition(levelDef);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapLoader] Error loading map from {path}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Cria um mapa padrão vazio
        /// </summary>
        public static MapDefinition CreateDefaultMap(int widthPixels, int heightPixels, int tileSize)
        {
            Console.WriteLine($"[MapLoader] Creating default map: {widthPixels}x{heightPixels} pixels");
            
            return new MapDefinition
            {
                MapWidth = widthPixels,
                MapHeight = heightPixels,
                TileSize = tileSize
            };
        }
    }
}
