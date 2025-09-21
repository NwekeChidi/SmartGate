using System.Text.Json;
using SmartGate.Api.ErrorHandling;

namespace SmartGate.Api.Tests.ErrorHandling;

public class StringTryParseConverterFactoryTests
{
    private readonly StringTryParseConverterFactory _factory = new();
    private readonly JsonSerializerOptions _options = new() { Converters = { new StringTryParseConverterFactory() } };

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(TestEnum))]
    public void CanConvert_SupportedTypes_ReturnsTrue(Type type)
    {
        _factory.CanConvert(type).Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(int?))]
    [InlineData(typeof(Guid?))]
    [InlineData(typeof(TestEnum?))]
    public void CanConvert_NullableSupportedTypes_ReturnsTrue(Type type)
    {
        _factory.CanConvert(type).Should().BeTrue();
    }

    [Fact]
    public void CanConvert_UnsupportedType_ReturnsFalse()
    {
        _factory.CanConvert(typeof(object)).Should().BeFalse();
    }

    [Fact]
    public void Deserialize_ValidInt_Success()
    {
        var json = "\"123\"";
        var result = JsonSerializer.Deserialize<int>(json, _options);
        result.Should().Be(123);
    }

    [Fact]
    public void Deserialize_ValidGuid_Success()
    {
        var guid = Guid.NewGuid();
        var json = $"\"{guid}\"";
        var result = JsonSerializer.Deserialize<Guid>(json, _options);
        result.Should().Be(guid);
    }

    [Fact]
    public void Deserialize_EmptyGuid_ThrowsException()
    {
        var json = $"\"{Guid.Empty}\"";
        var act = () => JsonSerializer.Deserialize<Guid>(json, _options);
        act.Should().Throw<JsonException>().WithMessage("Guid cannot be empty.");
    }

    [Fact]
    public void Deserialize_ValidEnum_Success()
    {
        var json = "\"Value1\"";
        var result = JsonSerializer.Deserialize<TestEnum>(json, _options);
        result.Should().Be(TestEnum.Value1);
    }

    [Fact]
    public void Deserialize_InvalidEnum_ThrowsException()
    {
        var json = "\"InvalidValue\"";
        var act = () => JsonSerializer.Deserialize<TestEnum>(json, _options);
        act.Should().Throw<JsonException>().WithMessage("Invalid TestEnum: 'InvalidValue'. Allowed: Value1, Value2.");
    }

    [Fact]
    public void Deserialize_NullableWithValue_Success()
    {
        var json = "\"123\"";
        var result = JsonSerializer.Deserialize<int?>(json, _options);
        result.Should().Be(123);
    }

    [Fact]
    public void Deserialize_NullableWithNull_Success()
    {
        var json = "null";
        var result = JsonSerializer.Deserialize<int?>(json, _options);
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyString_ThrowsException()
    {
        var json = "\"\"";
        var act = () => JsonSerializer.Deserialize<int>(json, _options);
        act.Should().Throw<JsonException>().WithMessage("Int32 cannot be empty.");
    }

    [Fact]
    public void Deserialize_NonStringToken_ThrowsException()
    {
        var json = "123";
        var act = () => JsonSerializer.Deserialize<int>(json, _options);
        act.Should().Throw<JsonException>().WithMessage("Expected string for Int32.");
    }

    [Fact]
    public void Serialize_Int_Success()
    {
        var value = 123;
        var result = JsonSerializer.Serialize(value, _options);
        result.Should().Be("\"123\"");
    }

    [Fact]
    public void Serialize_Enum_Success()
    {
        var value = TestEnum.Value1;
        var result = JsonSerializer.Serialize(value, _options);
        result.Should().Be("\"Value1\"");
    }

    [Fact]
    public void Serialize_NullableWithValue_Success()
    {
        int? value = 123;
        var result = JsonSerializer.Serialize(value, _options);
        result.Should().Be("\"123\"");
    }

    [Fact]
    public void Serialize_NullableWithNull_Success()
    {
        int? value = null;
        var result = JsonSerializer.Serialize(value, _options);
        result.Should().Be("null");
    }

    [Fact]
    public void Deserialize_InvalidInt_ThrowsException()
    {
        var json = "\"invalid\"";
        var act = () => JsonSerializer.Deserialize<int>(json, _options);
        act.Should().Throw<JsonException>().WithMessage("Invalid Int32: 'invalid'.");
    }

    [Fact]
    public void Deserialize_WhitespaceString_ThrowsException()
    {
        var json = "\"   \"";
        var act = () => JsonSerializer.Deserialize<int>(json, _options);
        act.Should().Throw<JsonException>().WithMessage("Int32 cannot be empty.");
    }

    [Fact]
    public void Serialize_NullValue_WritesNull()
    {
        string? value = null;
        var result = JsonSerializer.Serialize(value, _options);
        result.Should().Be("null");
    }

    [Fact]
    public void CreateConverter_NonNullableEnum_ReturnsEnumConverter()
    {
        var converter = _factory.CreateConverter(typeof(TestEnum), new JsonSerializerOptions());
        converter.Should().NotBeNull();
    }

    [Fact]
    public void CreateConverter_NonNullableType_ReturnsTryParseConverter()
    {
        var converter = _factory.CreateConverter(typeof(int), new JsonSerializerOptions());
        converter.Should().NotBeNull();
    }

    [Fact]
    public void CreateConverter_NullableType_ReturnsNullableConverter()
    {
        var converter = _factory.CreateConverter(typeof(int?), new JsonSerializerOptions());
        converter.Should().NotBeNull();
    }

    public enum TestEnum
    {
        Value1,
        Value2
    }
}