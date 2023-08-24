using LibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace LibraryManagement.Helpers.Serializers
{
    public class UserRentsConverter : JsonConverter<Dictionary<string, List<Item>>>
    {
        public override Dictionary<string, List<Item>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = document.RootElement;
                var dictionary = new Dictionary<string, List<Item>>();
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    string userName = property.Name;
                    List<Item> items = JsonSerializer.Deserialize<List<Item>>(property.Value.GetRawText(), options);
                    dictionary.Add(userName, items);
                }
                return dictionary;
            }
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, List<Item>> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                string userName = kvp.Key;
                writer.WritePropertyName(userName);
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
