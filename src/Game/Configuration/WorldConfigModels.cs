using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CubeSurvivor
{
    /// <summary>
    /// Modelo JSON para definição de abertura (porta) em uma zona segura.
    /// </summary>
    public sealed class JsonOpeningDefinition
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    /// <summary>
    /// Modelo JSON para definição de zona segura (casa, estrutura protegida).
    /// </summary>
    public sealed class JsonSafeZoneDefinition
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("opening")]
        public JsonOpeningDefinition Opening { get; set; }
    }

    /// <summary>
    /// Modelo JSON para definição de caixa/obstáculo.
    /// </summary>
    public sealed class JsonCrateDefinition
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("isDestructible")]
        public bool IsDestructible { get; set; }

        [JsonPropertyName("maxHealth")]
        public float MaxHealth { get; set; } = 50f;
    }

    /// <summary>
    /// Modelo JSON para definição de pickup (item coletável).
    /// </summary>
    public sealed class JsonPickupDefinition
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("amount")]
        public float Amount { get; set; }
    }

    /// <summary>
    /// Modelo JSON para definição de região de spawn de madeira.
    /// </summary>
    public sealed class JsonWoodSpawnRegion
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("maxActiveWood")]
        public int MaxActiveWood { get; set; }
    }

    /// <summary>
    /// Modelo JSON para definição de região de spawn de ouro.
    /// </summary>
    public sealed class JsonGoldSpawnRegion
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("maxActiveGold")]
        public int MaxActiveGold { get; set; }
    }

    /// <summary>
    /// Modelo JSON raiz para definição completa de um mundo/nível.
    /// Este formato será eventualmente substituído/complementado por TMX (Tiled) maps.
    /// </summary>
    public sealed class JsonWorldDefinition
    {
        [JsonPropertyName("mapWidth")]
        public int MapWidth { get; set; }

        [JsonPropertyName("mapHeight")]
        public int MapHeight { get; set; }

        [JsonPropertyName("safeZones")]
        public List<JsonSafeZoneDefinition> SafeZones { get; set; } = new();

        [JsonPropertyName("crates")]
        public List<JsonCrateDefinition> Crates { get; set; } = new();

        [JsonPropertyName("pickups")]
        public List<JsonPickupDefinition> Pickups { get; set; } = new();

        [JsonPropertyName("woodSpawnRegions")]
        public List<JsonWoodSpawnRegion> WoodSpawnRegions { get; set; } = new();

        [JsonPropertyName("goldSpawnRegions")]
        public List<JsonGoldSpawnRegion> GoldSpawnRegions { get; set; } = new();

        [JsonPropertyName("biomes")]
        public List<JsonBiomeDefinition> Biomes { get; set; } = new();
    }

    /// <summary>
    /// Modelo JSON para definição de bioma (área do mapa com propriedades específicas).
    /// </summary>
    public sealed class JsonBiomeDefinition
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("allowsEnemySpawns")]
        public bool AllowsEnemySpawns { get; set; } = true;

        [JsonPropertyName("treeDensity")]
        public int TreeDensity { get; set; } = 0;

        [JsonPropertyName("goldDensity")]
        public int GoldDensity { get; set; } = 0;

        [JsonPropertyName("texture")]
        public string Texture { get; set; }
    }

}

