using AntiSwearingChatBox.AI;
using System;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class AutoResponsesTest : TestBase
    {
        public AutoResponsesTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("AI-Powered Auto-Responses Test");
            Console.WriteLine("This test generates automated responses to messages with inappropriate content.");
            Console.WriteLine("Enter a message (include inappropriate content to trigger an auto-response):");
            string message = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("No message entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }
            
            string prompt = $"You are an AI moderation assistant for a family-friendly chat application. " +
                            $"When users send messages with inappropriate content, you generate automated responses. " +
                            $"Analyze the following message and if it contains inappropriate content, generate an appropriate system response.\n\n" +
                            $"User message: \"{message}\"\n\n" +
                            $"Return a JSON response with:\n" +
                            $"1. Whether the message contains inappropriate content (true/false)\n" +
                            $"2. Categories of inappropriate content detected (if any)\n" +
                            $"3. System auto-response message (appropriate for a family-friendly chat, explaining the issue)\n" +
                            $"4. Action recommendation (warning, mute, temporary ban, etc.)\n" +
                            $"5. Educational information to include in the response";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }
    }
} 