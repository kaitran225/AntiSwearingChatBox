using AntiSwearingChatBox.AI;
using System;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class ContextAwareFilteringTest : TestBase
    {
        public ContextAwareFilteringTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Context-Aware Filtering Test");
            Console.WriteLine("This test demonstrates how the system interprets messages based on context.");
            Console.WriteLine("Enter a message to test context-aware filtering (you can include ambiguous terms):");
            string message = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("No message entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }

            Console.WriteLine("\nProvide some previous messages for context (or press Enter to skip):");
            Console.WriteLine("Previous message 1:");
            string prevMsg1 = Console.ReadLine() ?? string.Empty;
            
            Console.WriteLine("Previous message 2:");
            string prevMsg2 = Console.ReadLine() ?? string.Empty;

            string conversation = string.Empty;
            if (!string.IsNullOrWhiteSpace(prevMsg1))
            {
                conversation += $"User1: {prevMsg1}\n";
            }
            if (!string.IsNullOrWhiteSpace(prevMsg2))
            {
                conversation += $"User2: {prevMsg2}\n";
            }
            
            string prompt = $"Perform context-aware moderation on the following conversation for a family-friendly chat application. " +
                            $"Consider context when determining if the latest message contains inappropriate content.\n\n" +
                            $"Previous conversation:\n{conversation}\n" +
                            $"Latest message: \"{message}\"\n\n" +
                            $"Return a JSON response with:\n" +
                            $"1. Whether the latest message is inappropriate in this context\n" +
                            $"2. Explanation of the context consideration\n" +
                            $"3. Filtered version of the message (if needed)\n" +
                            $"4. Whether the message would be considered inappropriate without context";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }
    }
} 