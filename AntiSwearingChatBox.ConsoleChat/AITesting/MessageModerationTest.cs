using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class MessageModerationTest : TestBase
    {
        public MessageModerationTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Basic Message Moderation Test");
            Console.WriteLine("Enter a message to moderate (try including inappropriate language to test moderation):");
            string message = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("No message entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }
            
            string prompt = $"Moderate the following message for a family-friendly chat application. " +
                            $"Check for profanity, inappropriate content, hate speech, threats, or other harmful content:\n\n" +
                            $"\"{message}\"\n\n" +
                            $"Return a JSON response with moderation results and if needed, a filtered version of the message.";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }

        private void WaitForKeyPress()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
} 