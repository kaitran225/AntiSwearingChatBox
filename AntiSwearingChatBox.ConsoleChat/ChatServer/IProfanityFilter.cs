using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.ChatServer
{
    public interface IProfanityFilter
    {
        Task<bool> ContainsProfanityAsync(string text);
        Task<string> FilterTextAsync(string text);
    }
} 