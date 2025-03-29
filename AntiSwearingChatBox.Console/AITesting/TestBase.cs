using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public abstract class TestBase : ITestModule
    {
        protected readonly GeminiService GeminiService;

        protected TestBase(GeminiService geminiService)
        {
            GeminiService = geminiService;
        }

        public abstract Task RunAsync();

        protected void WaitForKeyPress()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        protected string GetUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? string.Empty;
        }

        protected void PrintTitle(string title)
        {
            Console.WriteLine($"\n=== {title} ===");
        }

        protected void PrintProcessing(string message)
        {
            Console.WriteLine($"\n{message}...");
        }

        protected async Task ProcessAndPrintResponse(string prompt)
        {
            PrintProcessing("Processing");
            string response = await GeminiService.GenerateJsonResponseAsync(prompt);
            Console.WriteLine("\nJSON Response:");
            AITestConsole.FormatAndPrintJson(response);
        }
    }
} 