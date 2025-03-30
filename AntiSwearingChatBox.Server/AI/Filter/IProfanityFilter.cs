namespace AntiSwearingChatBox.AI.Interfaces;

/// <summary>
/// Interface for profanity filtering services
/// </summary>
public interface IProfanityFilter
{
    /// <summary>
    /// Checks if the input text contains profanity
    /// </summary>
    /// <param name="text">Text to check</param>
    /// <returns>True if profanity is detected, otherwise false</returns>
    Task<bool> ContainsProfanityAsync(string text);

    /// <summary>
    /// Filters profanity from the input text
    /// </summary>
    /// <param name="text">Text to filter</param>
    /// <returns>Filtered text with profanity replaced</returns>
    Task<string> FilterTextAsync(string text);
    
    /// <summary>
    /// Filters profanity from text and returns both filtered text and whether it was modified
    /// </summary>
    /// <param name="text">Text to filter</param>
    /// <returns>Tuple containing filtered text and a flag indicating whether it was modified</returns>
    (string filteredText, bool wasModified) FilterProfanity(string text);
} 