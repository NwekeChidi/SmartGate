using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartGate.Api.ErrorHandling;

public delegate bool TryParseDelegate(string input, out object? result);

public sealed class StringTryParseConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type t)
    {
        var underlying = Nullable.GetUnderlyingType(t) ?? t;
        return underlying.IsEnum || FindTryParse(underlying) is not null;
    }

    public override JsonConverter CreateConverter(Type t, JsonSerializerOptions _)
    {
        var underlyingType = Nullable.GetUnderlyingType(t);
        var targetType = underlyingType ?? t;
        
        var converter = targetType.IsEnum
            ? (JsonConverter)Activator.CreateInstance(typeof(EnumConverter<>).MakeGenericType(targetType))!
            : (JsonConverter)Activator.CreateInstance(typeof(TryParseConverter<>).MakeGenericType(targetType), FindTryParse(targetType)!)!;

        return underlyingType is not null
            ? (JsonConverter)Activator.CreateInstance(typeof(NullableConverter<>).MakeGenericType(underlyingType), converter)!
            : converter;
    }

    private static TryParseDelegate? FindTryParse(Type t)
    {
        var methods = new[]
        {
            (t.GetMethod("TryParse", [typeof(string), typeof(IFormatProvider), t.MakeByRefType()]), 2),
            (t.GetMethod("TryParse", [typeof(string), t.MakeByRefType()]), 1)
        };

        foreach (var (method, resultIndex) in methods)
        {
            if (method?.ReturnType == typeof(bool))
                return (string s, out object? boxed) =>
                {
                    var args = resultIndex == 2 ? [s, CultureInfo.InvariantCulture, null] : new object?[] { s, null };
                    var ok = (bool)method.Invoke(null, args)!;
                    boxed = ok ? args[resultIndex] : null;
                    return ok;
                };
        }

        return null;
    }

    private static string ValidateAndGetString(Utf8JsonReader reader, string typeName)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected string for {typeName}.");

        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            throw new JsonException($"{typeName} cannot be empty.");

        return raw!;
    }

    private sealed class TryParseConverter<T>(TryParseDelegate tryParse) : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var raw = ValidateAndGetString(reader, typeof(T).Name);
            
            if (!tryParse(raw, out var result) || result is null)
                throw new JsonException($"Invalid {typeof(T).Name}: '{raw}'.");

            if (typeof(T) == typeof(Guid) && (Guid)result == Guid.Empty)
                throw new JsonException("Guid cannot be empty.");

            return (T)result;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteStringValue(value?.ToString());
    }

    private sealed class EnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var raw = ValidateAndGetString(reader, typeof(T).Name);
            
            if (Enum.TryParse<T>(raw, true, out var value))
                return value;

            var allowed = string.Join(", ", Enum.GetNames<T>());
            throw new JsonException($"Invalid {typeof(T).Name}: '{raw}'. Allowed: {allowed}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }

    private sealed class NullableConverter<T>(JsonConverter<T> inner) : JsonConverter<T?> where T : struct
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null ? null : inner.Read(ref reader, typeof(T), options);

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                inner.Write(writer, value.Value, options);
            else
                writer.WriteNullValue();
        }
    }
}
