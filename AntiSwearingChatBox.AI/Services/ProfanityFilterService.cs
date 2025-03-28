using AntiSwearingChatBox.Core.Interfaces;
using System.Text.RegularExpressions;

namespace AntiSwearingChatBox.AI.Services;

public class ProfanityFilterService : IProfanityFilter
{
    // Basic list of profanity words to filter
    // In a real implementation, this would be more sophisticated and extensive
    private static readonly string[] _profanityWords = 
    {
        "badword", "swear", "profanity"
    };

    private static readonly Regex _profanityRegex;

    static ProfanityFilterService()
    {
        // Create a regex pattern for all profanity words with word boundaries
        var pattern = @"\b(" + string.Join("|", _profanityWords.Select(Regex.Escape)) + @")\b";
        _profanityRegex = new Regex(pattern, RegexOptions.IgnoreCase);
    }

    /// <inheritdoc />
    public Task<bool> ContainsProfanityAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult(false);

        return Task.FromResult(_profanityRegex.IsMatch(text));
    }

    /// <inheritdoc />
    public Task<string> FilterTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult(string.Empty);

        // Replace profanity with asterisks
        var filtered = _profanityRegex.Replace(text, match => 
        {
            return new string('*', match.Length);
        });

        return Task.FromResult(filtered);
    }
} 