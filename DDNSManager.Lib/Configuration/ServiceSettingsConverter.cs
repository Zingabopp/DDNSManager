using DDNSManager.Lib.Services;
using System;
using System.Collections.Generic;
using System.Text;
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
                var tokenType = reader.TokenType;
                if (tokenType == JsonTokenType.EndArray)
                    break;
                if (tokenType == JsonTokenType.StartObject)
                {
                    var setting = ServiceSettingsConverter.Default.Read(ref reader, typeof(IServiceSettings), options);
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
            foreach (var serviceSettings in value)
            {
                object obj = (object)serviceSettings;
                JsonSerializer.Serialize(writer, obj, options);
            }
            writer.WriteEndArray();
        }
    }

    public class ServiceSettingsConverter : JsonConverter<IServiceSettings>
    {
        public static ServiceSettingsConverter Default = new ServiceSettingsConverter();
        private static Dictionary<string, Type> _typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { GoogleDnsService.ServiceId, typeof(GoogleDnsSettings) }
        };

        public static void RegisterType(string serviceId, Type type)
        {
            _typeMap[serviceId] = type;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IServiceSettings).IsAssignableFrom(typeToConvert);
        }
        public override IServiceSettings? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of an object.");

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            string? serviceId = jsonDoc.RootElement.GetProperty(nameof(IServiceSettings.ServiceId)).GetString();
            if (serviceId != null && _typeMap.TryGetValue(serviceId, out Type type))
            {
                return jsonDoc.Deserialize(type, options) as IServiceSettings;
            }
            throw new JsonException($"Unknown ServiceId: '{serviceId ?? "NULL"}'");
        }

        public override void Write(Utf8JsonWriter writer, IServiceSettings value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            JsonSerializer.Serialize(writer, value, options);
            writer.WriteEndObject();
        }
    }
}
