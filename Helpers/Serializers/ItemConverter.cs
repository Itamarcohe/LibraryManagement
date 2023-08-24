using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LibraryManagement.Models;

namespace LibraryManagement.Helpers
{
    public class ItemConverter : JsonConverter<Item>
    {
        public override Item Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;
                var hasAuthorProperty = root.TryGetProperty("Author", out _);

                if (hasAuthorProperty)
                {
                    return JsonSerializer.Deserialize<Book>(root.GetRawText(), options);
                }
                else
                {
                    return JsonSerializer.Deserialize<Magazine>(root.GetRawText(), options);
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, Item value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
//}
