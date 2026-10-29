using System.Text.RegularExpressions;

namespace PS2Home.Core;

/// <summary>
/// Helper for deriving human-friendly display titles from game image filenames.
/// </summary>
public static partial class TitleFormatter
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleWhitespaceRegex();

    public static string Format(string? filePathOrName)
    {
        if (string.IsNullOrWhiteSpace(filePathOrName))
            return string.Empty;

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePathOrName);
        if (string.IsNullOrWhiteSpace(fileNameWithoutExt))
            return filePathOrName.Trim();

        // Replace underscores with spaces
        var cleaned = fileNameWithoutExt.Replace('_', ' ');

        // Collapse multiple spaces into one
        cleaned = MultipleWhitespaceRegex().Replace(cleaned, " ").Trim();

        return string.IsNullOrEmpty(cleaned) ? fileNameWithoutExt : cleaned;
    }
}
