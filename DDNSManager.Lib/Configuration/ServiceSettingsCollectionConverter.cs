using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DDNSManager.Lib.Configuration
{
    public class ServiceSettingsCollectionConverter : JsonConverter<IEnumerable<IServiceSettings>>
    {

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IEnumerable<IServiceSettings>).IsAssignableFrom(typeToConvert);
        }
        public override IEnumerable<IServiceSettings>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of JsonArray");
            List<IServiceSettings> list = new List<IServiceSettings>();

            while (reader.Read())
            {
                JsonTokenType tokenType = reader.TokenType;
                if (tokenType == JsonTokenType.EndArray)
                    break;
                if (tokenType == JsonTokenType.StartObject)
                {
                    IServiceSettings? setting = ServiceSettingsConverter.Default.Read(ref reader, typeof(IServiceSettings), options);
                    if (setting != null)
                        list.Add(setting);
                    else
                        throw new JsonException("A setting in the list deserialized to null.");
                }
            }
            return list;
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<IServiceSettings> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (IServiceSettings? serviceSettings in value)
            {
                object obj = (object)serviceSettings;
                JsonSerializer.Serialize(writer, obj, options);
            }
            writer.WriteEndArray();
        }
    }

}
