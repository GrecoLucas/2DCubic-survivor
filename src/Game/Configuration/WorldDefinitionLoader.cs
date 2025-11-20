using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using CubeSurvivor.World.Biomes;

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

                // Converter pickups
                foreach (var pickup in jsonWorld.Pickups)
                {
                    level.Pickups.Add(new PickupDefinition
                    {
                        Position = new Vector2(pickup.X, pickup.Y),
                        Type = pickup.Type,
                        Amount = pickup.Amount
                    });
                }

                Console.WriteLine($"[WorldLoader] - {jsonWorld.Pickups.Count} pickups carregados");

                // Converter regiões de spawn de madeira
                foreach (var region in jsonWorld.WoodSpawnRegions)
                {
                    level.WoodSpawnRegions.Add(new WoodSpawnRegionDefinition
                    {
                        Area = new Rectangle(region.X, region.Y, region.Width, region.Height),
                        MaxActiveWood = region.MaxActiveWood
                    });
                }

                Console.WriteLine($"[WorldLoader] - {jsonWorld.WoodSpawnRegions.Count} regiões de spawn de madeira carregadas");
                Console.WriteLine("[WorldLoader] ✓ Mundo carregado com sucesso!");

                // Armazenar dimensões do mapa (pode ser usado para atualizar GameConfig em runtime)
                level.MapWidth = jsonWorld.MapWidth;
                level.MapHeight = jsonWorld.MapHeight;

                // Converter biomas (se houver)
                if (jsonWorld.Biomes != null && jsonWorld.Biomes.Count > 0)
                {
                    foreach (var jb in jsonWorld.Biomes)
                    {
                        var rect = new Rectangle(jb.X, jb.Y, jb.Width, jb.Height);
                        CubeSurvivor.World.Biomes.BiomeType type = CubeSurvivor.World.Biomes.BiomeType.Unknown;
                        if (!string.IsNullOrWhiteSpace(jb.Type))
                        {
                            if (!Enum.TryParse<CubeSurvivor.World.Biomes.BiomeType>(jb.Type, true, out type))
                            {
                                type = CubeSurvivor.World.Biomes.BiomeType.Unknown;
                            }
                        }

                        var textureKey = jb.Texture;
                        if (string.IsNullOrWhiteSpace(textureKey) && !string.IsNullOrWhiteSpace(jb.Type))
                        {
                            textureKey = jb.Type.ToLower() + ".png";
                        }

                        level.Biomes.Add(new BiomeDefinition
                        {
                            Area = rect,
                            Type = type,
                            AllowsEnemySpawns = jb.AllowsEnemySpawns,
                            TreeDensity = jb.TreeDensity,
                            TextureKey = textureKey
                        });
                    }

                    Console.WriteLine($"[WorldLoader] - {jsonWorld.Biomes.Count} biomas carregados");
                }

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

