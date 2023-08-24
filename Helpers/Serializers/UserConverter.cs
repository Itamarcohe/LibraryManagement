using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using LibraryManagement.Models;

namespace LibraryManagement.Helpers.Serializers
{
    public class UserConverter : JsonConverter<User>
    {
        public override User Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = document.RootElement;
                string name = root.GetProperty("Name").GetString();
                string password = root.GetProperty("Password").GetString();
                UserType userType = Enum.Parse<UserType>(root.GetProperty("UserType").GetString(), ignoreCase: true);
                return new User
                {
                    Name = name,
                    Password = password,
                    UserType = userType
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", value.Name);
            writer.WriteString("Password", value.Password);
            writer.WriteString("UserType", value.UserType.ToString());
            writer.WriteEndObject();
        }
    }


}
