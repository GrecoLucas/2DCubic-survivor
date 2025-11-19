using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace CubeSurvivor
{
    /// <summary>
    /// Carrega definições de mundo a partir de arquivos JSON.
    /// Futuramente pode ser estendido para carregar mapas TMX do Tiled.
    /// </summary>
    public static class WorldDefinitionLoader
    {
        /// <summary>
        /// Carrega uma LevelDefinition a partir de um arquivo JSON.
        /// </summary>
        /// <param name="path">Caminho para o arquivo JSON (relativo ou absoluto)</param>
        /// <returns>LevelDefinition preenchida com os dados do JSON</returns>
        public static LevelDefinition LoadFromJson(string path)
        {
            var level = new LevelDefinition();

            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[WorldLoader] ⚠ Arquivo não encontrado: {path}");
                    return level;
                }

                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var jsonWorld = JsonSerializer.Deserialize<JsonWorldDefinition>(json, options);

                if (jsonWorld == null)
                {
                    Console.WriteLine("[WorldLoader] ⚠ Falha ao deserializar JSON");
                    return level;
                }

                Console.WriteLine($"[WorldLoader] Carregando mundo: {jsonWorld.MapWidth}x{jsonWorld.MapHeight}");

                // Converter caixas/obstáculos
                foreach (var crate in jsonWorld.Crates)
                {
                    level.Crates.Add(new CrateDefinition
                    {
                        Position = new Vector2(crate.X, crate.Y),
                        IsDestructible = crate.IsDestructible,
                        MaxHealth = crate.MaxHealth
                    });
                }

                Console.WriteLine($"[WorldLoader] - {jsonWorld.Crates.Count} caixas carregadas");

                // Converter zonas seguras
                foreach (var zone in jsonWorld.SafeZones)
                {
                    var area = new Rectangle(zone.X, zone.Y, zone.Width, zone.Height);

                    var openingRect = zone.Opening != null
                        ? new Rectangle(zone.Opening.X, zone.Opening.Y, zone.Opening.Width, zone.Opening.Height)
                        : Rectangle.Empty;

                    level.SafeZones.Add(new SafeZoneDefinition
                    {
                        Area = area,
                        OpeningArea = openingRect
                    });
                }

                Console.WriteLine($"[WorldLoader] - {jsonWorld.SafeZones.Count} zonas seguras carregadas");

                // TODO: Converter pickups quando o sistema estiver pronto
                // foreach (var pickup in jsonWorld.Pickups) { ... }

                Console.WriteLine("[WorldLoader] ✓ Mundo carregado com sucesso!");

                // Armazenar dimensões do mapa (pode ser usado para atualizar GameConfig em runtime)
                level.MapWidth = jsonWorld.MapWidth;
                level.MapHeight = jsonWorld.MapHeight;

                return level;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldLoader] ✗ Erro ao carregar mundo: {ex.Message}");
                return level;
            }
        }
    }
}

