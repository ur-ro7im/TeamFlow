namespace TeamFlow.API.Helpers;

/// <summary>
/// Enum.TryParse accepts numeric strings such as "99" even when the value is not a
/// defined member. This wrapper only succeeds for names or numbers that are defined.
/// </summary>
public static class EnumParser
{
    public static bool TryParseDefined<TEnum>(
        string? value,
        bool ignoreCase,
        out TEnum result)
        where TEnum : struct, Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Enum.TryParse(value.Trim(), ignoreCase, out TEnum parsed))
        {
            return false;
        }

        if (!Enum.IsDefined(parsed))
        {
            return false;
        }

        result = parsed;
        return true;
    }
}
