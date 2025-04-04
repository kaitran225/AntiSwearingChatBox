namespace AntiSwearingChatBox.AI.Filter;

public interface IProfanityFilter
{
    string FilterProfanity(string text);
    Task<string> FilterProfanityAsync(string text);
    Task<bool> ContainsProfanityAsync(string text);
} 