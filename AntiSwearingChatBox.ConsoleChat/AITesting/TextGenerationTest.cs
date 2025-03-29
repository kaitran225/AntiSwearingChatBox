using AntiSwearingChatBox.AI;
using System;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class TextGenerationTest : TestBase
    {
        public TextGenerationTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Text Generation Test");
            string prompt = GetUserInput("Enter a prompt: ");

            if (string.IsNullOrWhiteSpace(prompt))
            {
                Console.WriteLine("No prompt entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }

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