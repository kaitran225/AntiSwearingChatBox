using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class SentimentAnalysisTest : TestBase
    {
        public SentimentAnalysisTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Sentiment Analysis & Toxicity Detection Test");
            Console.WriteLine("This test analyzes the sentiment and checks for toxicity in a message.");
            Console.WriteLine("Enter a message to analyze:");
            string message = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("No message entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }
            
            string prompt = $"Analyze the following message for a family-friendly chat application. " +
                            $"Perform sentiment analysis and toxicity detection.\n\n" +
                            $"Message: \"{message}\"\n\n" +
                            $"Return a JSON response with:\n" +
                            $"1. Sentiment (positive, neutral, negative)\n" +
                            $"2. Sentiment score (0-1 scale)\n" +
                            $"3. Toxicity detection (true/false)\n" +
                            $"4. Toxicity score (0-1 scale)\n" +
                            $"5. Categories of toxicity detected (if any): profanity, hate speech, harassment, etc.\n" +
                            $"6. Explanation of the analysis\n" +
                            $"7. Warning message if toxicity detected";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }
    }
} 