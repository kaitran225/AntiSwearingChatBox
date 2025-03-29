using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class LanguageSpecificModerationTest : TestBase
    {
        public LanguageSpecificModerationTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Language-Specific Moderation Test");
            Console.WriteLine("This test detects and filters inappropriate content in multiple languages.");
            
            Console.WriteLine("Available languages: English, Spanish, French, German, Italian, Portuguese");
            Console.Write("Enter language for testing: ");
            string language = Console.ReadLine() ?? "English";
            
            Console.WriteLine($"\nEnter a message in {language} to moderate (include inappropriate content to test filtering):");
            string message = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("No message entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }
            
            string prompt = $"You are an AI moderator for a family-friendly chat application that supports multiple languages. " +
                            $"Analyze the following message in {language} for inappropriate content.\n\n" +
                            $"Message: \"{message}\"\n\n" +
                            $"Return a JSON response with:\n" +
                            $"1. Detected language (confirm or correct)\n" +
                            $"2. Whether the message contains inappropriate content (true/false)\n" +
                            $"3. Categories of inappropriate content detected (if any)\n" +
                            $"4. Filtered version of the message in the same language\n" +
                            $"5. Explanation (in English) of what was inappropriate\n" +
                            $"6. Cultural context that might be relevant to the moderation decision";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }
    }
} 