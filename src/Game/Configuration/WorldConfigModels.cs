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
    }
}

