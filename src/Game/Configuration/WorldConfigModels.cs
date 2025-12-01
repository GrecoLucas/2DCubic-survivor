using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CubeSurvivor
{
    /// <summary>
    /// Modelo JSON para definição de abertura (porta) em uma zona segura.
    /// </summary>
    public sealed class JsonRectDefinition
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

    public sealed class JsonPositionDefinition
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

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
        [JsonPropertyName("area")]
        public JsonRectDefinition Area { get; set; }

        [JsonPropertyName("openingArea")]
        public JsonRectDefinition OpeningArea { get; set; }
    }

    /// <summary>
    /// Modelo JSON para definição de caixa/obstáculo.
    /// </summary>
    public sealed class JsonCrateDefinition
    {
        [JsonPropertyName("position")]
        public JsonPositionDefinition Position { get; set; }

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
        [JsonPropertyName("position")]
        public JsonPositionDefinition Position { get; set; }

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
        [JsonPropertyName("area")]
        public JsonRectDefinition Area { get; set; }

        [JsonPropertyName("maxActiveWood")]
        public int MaxActiveWood { get; set; }
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

        [JsonPropertyName("biomes")]
        public List<JsonBiomeDefinition> Biomes { get; set; } = new();
    }

    /// <summary>
    /// Modelo JSON para definição de bioma (área do mapa com propriedades específicas).
    /// </summary>
    public sealed class JsonBiomeDefinition
    {
        [JsonPropertyName("area")]
        public JsonRectDefinition Area { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("allowsEnemySpawns")]
        public bool AllowsEnemySpawns { get; set; } = true;

        [JsonPropertyName("treeDensity")]
        public int TreeDensity { get; set; } = 0;

        [JsonPropertyName("textureKey")]
        public string TextureKey { get; set; }

        [JsonPropertyName("allowedEnemies")]
        public List<string> AllowedEnemies { get; set; } = new();
    }

}

