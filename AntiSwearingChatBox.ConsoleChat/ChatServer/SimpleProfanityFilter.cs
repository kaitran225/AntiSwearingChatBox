using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.ChatServer
{
    public class SimpleProfanityFilter : IProfanityFilter
    {
        private static readonly string[] _badWords = new[] { "badword", "damn", "swear", "profanity" };
        
        public Task<bool> ContainsProfanityAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(false);
                
            return Task.FromResult(_badWords.Any(word => 
                text.ToLower().Contains(word.ToLower())));
        }
        
        public Task<string> FilterTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(string.Empty);
                
            string filtered = text;
            foreach (var word in _badWords)
            {
                filtered = Regex.Replace(
                    filtered,
                    $@"\b{word}\b",
                    new string('*', word.Length),
                    RegexOptions.IgnoreCase);
            }
            
            return Task.FromResult(filtered);
        }
    }
} 