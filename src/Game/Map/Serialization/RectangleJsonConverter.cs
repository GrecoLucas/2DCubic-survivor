using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Game.Map.Serialization
{
    /// <summary>
    /// Custom JSON converter for Rectangle that handles both X/Y/Width/Height and Left/Top/Right/Bottom formats.
    /// Ensures proper serialization/deserialization of Rectangle for region areas.
    /// </summary>
    public sealed class RectangleJsonConverter : JsonConverter<Rectangle>
    {
        public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var obj = doc.RootElement;

            // 1) Novo formato: x, y, width, height (camelCase ou PascalCase)
            if (TryGetInt(obj, "x", out int x) &&
                TryGetInt(obj, "y", out int y) &&
                TryGetInt(obj, "width", out int w) &&
                TryGetInt(obj, "height", out int h))
            {
                return new Rectangle(x, y, w, h);
            }

            // 2) Formato antigo: left, top, right, bottom
            if (TryGetInt(obj, "left", out int left) &&
                TryGetInt(obj, "top", out int top) &&
                TryGetInt(obj, "right", out int right) &&
                TryGetInt(obj, "bottom", out int bottom))
            {
                return new Rectangle(left, top, right - left, bottom - top);
            }

            // 3) Fallback total
            return Rectangle.Empty;
        }

        public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteNumber("width", value.Width);
            writer.WriteNumber("height", value.Height);
            writer.WriteEndObject();
        }

        private static bool TryGetInt(JsonElement obj, string name, out int value)
        {
            // camelCase
            if (obj.TryGetProperty(name, out var p) && p.TryGetInt32(out value))
                return true;

            // PascalCase
            string pascal = char.ToUpperInvariant(name[0]) + name.Substring(1);
            if (obj.TryGetProperty(pascal, out p) && p.TryGetInt32(out value))
                return true;

            value = 0;
            return false;
        }
    }
}

