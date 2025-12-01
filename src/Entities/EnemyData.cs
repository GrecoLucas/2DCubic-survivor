using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CubeSurvivor.Entities
{
    public class EnemyData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("width")]
        public float Width { get; set; }

        [JsonPropertyName("height")]
        public float Height { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("speed")]
        public float Speed { get; set; }

        [JsonPropertyName("health")]
        public float Health { get; set; }

        [JsonPropertyName("damage")]
        public float Damage { get; set; }

        [JsonPropertyName("lootTable")]
        public List<LootItem> LootTable { get; set; }
    }

    public class LootItem
    {
        [JsonPropertyName("item")]
        public string Item { get; set; }

        [JsonPropertyName("chance")]
        public float Chance { get; set; }
    }
}
