using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class ProfanityDetectionTest : TestBase
    {
        public ProfanityDetectionTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Profanity Detection & Warning Test");
            Console.WriteLine("Enter a message to check for profanity and generate appropriate warnings:");
            string message = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("No message entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }
            
            string prompt = $"Analyze the following message for a family-friendly chat application. " +
                            $"If it contains profanity or inappropriate language, generate a user warning with an explanation of what was inappropriate.\n\n" +
                            $"Message: \"{message}\"\n\n" +
                            $"Return a JSON response with the following:\n" +
                            $"1. Detection results (contains_profanity: true/false)\n" +
                            $"2. Filtered message (if needed)\n" +
                            $"3. Warning message appropriate for the user (if needed)\n" +
                            $"4. Severity level (low, medium, high) if profanity is detected\n" +
                            $"5. Categories of inappropriate content detected (profanity, hate speech, threats, etc.)";

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