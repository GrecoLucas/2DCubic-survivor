using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Custom JSON converter for 2D arrays (System.Text.Json doesn't support them natively).
    /// Converts 2D arrays to/from nested JSON arrays.
    /// </summary>
    public class Array2DJsonConverter<T> : JsonConverter<T[,]>
    {
        public override T[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array");

            var rows = new System.Collections.Generic.List<T[]>();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                    
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException("Expected start of row array");
                
                var row = new System.Collections.Generic.List<T>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;
                    
                    T value;
                    if (typeof(T).IsEnum)
                    {
                        value = (T)Enum.ToObject(typeof(T), reader.GetInt32());
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        value = (T)(object)reader.GetInt32();
                    }
                    else
                    {
                        throw new JsonException($"Unsupported type: {typeof(T)}");
                    }
                    row.Add(value);
                }
                rows.Add(row.ToArray());
            }
            
            if (rows.Count == 0)
                return new T[0, 0];
            
            int height = rows.Count;
            int width = rows[0].Length;
            var result = new T[height, width];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[y, x] = rows[y][x];
                }
            }
            
            return result;
        }

        public override void Write(Utf8JsonWriter writer, T[,] value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            
            int height = value.GetLength(0);
            int width = value.GetLength(1);
            
            writer.WriteStartArray();
            for (int y = 0; y < height; y++)
            {
                writer.WriteStartArray();
                for (int x = 0; x < width; x++)
                {
                    if (typeof(T).IsEnum)
                    {
                        writer.WriteNumberValue(Convert.ToInt32(value[y, x]));
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        writer.WriteNumberValue((int)(object)value[y, x]);
                    }
                    else
                    {
                        throw new JsonException($"Unsupported type: {typeof(T)}");
                    }
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }
    }
}





