using AntiSwearingChatBox.AI;
using System;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class AlternativeSuggestionTest : TestBase
    {
        public AlternativeSuggestionTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("AI-Based Alternative Suggestion Test");
            Console.WriteLine("This test suggests alternative ways to express a message that contains inappropriate content.");
            Console.WriteLine("Enter a message with inappropriate content to get alternative suggestions:");
            string message = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("No message entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }
            
            string prompt = $"You are an AI assistant helping users in a family-friendly chat application. " +
                            $"When a user tries to send a message with inappropriate content, you suggest alternative ways to express the same meaning without using inappropriate language.\n\n" +
                            $"Original message: \"{message}\"\n\n" +
                            $"Return a JSON response with:\n" +
                            $"1. Whether the message contains inappropriate content (true/false)\n" +
                            $"2. If inappropriate, provide 3 alternative ways to express the same idea appropriately\n" +
                            $"3. Explanation of why the original message is inappropriate (if it is)\n" +
                            $"4. Educational note about communication in family-friendly environments";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }
    }
} 