namespace One.SimpleLog.Extensions;

internal static class StringExtensions
{
    internal static int ToInt(this string input)
    {
        _ = int.TryParse(input, out int result);
        return result;
    }
}