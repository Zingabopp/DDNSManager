using DDNSManager.Lib.ServiceConfiguration;
using DDNSManager.Lib.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static DDNSManager.Lib.DefaultServiceRegistration;
namespace DDNSManager.Lib.Configuration
{
    public class ServiceSettingsConverter : JsonConverter<IServiceSettings>
    {
        public static ServiceSettingsConverter Default = new ServiceSettingsConverter();

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
            if (serviceId != null && TryGetSettingsType(serviceId, out Type type))
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
