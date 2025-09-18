using System.Text.RegularExpressions;

namespace SmartGate.Domain.Common;

public static partial class Normalization
{
    public static string NormalizePlateOrUnit(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var upper = input.Trim().ToUpperInvariant();
        var stripped = NonAlphaNumeric().Replace(upper, "");
        return stripped;
    }

    [GeneratedRegex("[^A-Z0-9]")]
    private static partial Regex NonAlphaNumeric();
}