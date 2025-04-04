using System.Threading.Tasks;

namespace AntiSwearingChatBox.AI.Filter;

/// <summary>
/// Interface for profanity filter service that detects and filters out inappropriate language
/// </summary>
public interface IProfanityFilter
{
    /// <summary>
    /// Filters profanity from the input text
    /// </summary>
    /// <param name="text">The text to check for profanity</param>
    /// <returns>The filtered text with profanity replaced</returns>
    string FilterProfanity(string text);
    
    /// <summary>
    /// Asynchronously filters profanity from the input text
    /// </summary>
    /// <param name="text">The text to check for profanity</param>
    /// <returns>The filtered text with profanity replaced</returns>
    Task<string> FilterProfanityAsync(string text);
    
    /// <summary>
    /// Checks if the input text contains profanity
    /// </summary>
    /// <param name="text">The text to check for profanity</param>
    /// <returns>True if the text contains profanity, false otherwise</returns>
    Task<bool> ContainsProfanityAsync(string text);
} 