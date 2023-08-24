using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;
using LibraryManagement.Models;

namespace LibraryManagement.Helpers.Serializers
{
    public class LibraryCashBoxConverter : JsonConverter<MyCashBox>

    {
        public override MyCashBox Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("TotalCash", out JsonElement totalCashElement) && totalCashElement.TryGetDouble(out double totalCash))
                {
                    return new MyCashBox { TotalCash = totalCash };
                }
                else
                {
                    return null;
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, MyCashBox value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("TotalCash", value.TotalCash);
            writer.WriteEndObject();
        }

    }
}
