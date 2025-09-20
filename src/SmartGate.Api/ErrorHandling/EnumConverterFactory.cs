using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartGate.Api.ErrorHandling;

public sealed class StringEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsEnum || (Nullable.GetUnderlyingType(typeToConvert)?.IsEnum ?? false);

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        var converterType = typeof(StringEnumConverter<>).MakeGenericType(t);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class StringEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected a string for {typeof(TEnum).Name}.");

            var raw = reader.GetString() ?? "";
            if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var value))
                return value;

            var allowed = string.Join(", ", Enum.GetNames(typeof(TEnum)));
            throw new JsonException($"Invalid {typeof(TEnum).Name} value '{raw}'. Allowed: {allowed}.");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
