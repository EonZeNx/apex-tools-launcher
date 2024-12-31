namespace ApexToolsLauncher.Core.Extensions;

public static class StringExtensions
{
    public static string RemoveAll(this string value, IEnumerable<string> characters)
    {
        return characters.Aggregate(value, (current, character) => current.Replace(character, ""));
    }
}