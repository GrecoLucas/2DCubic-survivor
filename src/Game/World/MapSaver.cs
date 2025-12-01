using System;
using System.IO;
using System.Text.Json;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Salva definições de mapas em arquivos JSON.
    /// </summary>
    public static class MapSaver
    {
        /// <summary>
        /// Salva um mapa em arquivo JSON
        /// </summary>
        public static bool Save(string path, MapDefinition mapDef)
        {
            try
            {
                // Converter MapDefinition para LevelDefinition
                var levelDef = mapDef.ToLevelDefinition();
                
                // Serializar para JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                string json = JsonSerializer.Serialize(levelDef, options);
                
                // Criar diretório se não existir
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                
                // Salvar arquivo
                File.WriteAllText(path, json);
                
                Console.WriteLine($"[MapSaver] Map saved to {path}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapSaver] Error saving map to {path}: {ex.Message}");
                return false;
            }
        }
    }
}
