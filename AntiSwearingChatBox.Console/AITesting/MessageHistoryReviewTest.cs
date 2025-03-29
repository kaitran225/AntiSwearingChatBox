using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class MessageHistoryReviewTest : TestBase
    {
        public MessageHistoryReviewTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Message History Review Test");
            Console.WriteLine("This test analyzes a chat history to detect patterns of inappropriate content.");
            
            var messageHistory = new List<string>();
            Console.WriteLine("Enter up to 5 messages to analyze (one per line).");
            Console.WriteLine("Press Enter on an empty line when finished.");
            
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"Message {i+1}: ");
                string msg = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(msg))
                    break;
                    
                messageHistory.Add(msg);
            }
            
            if (messageHistory.Count == 0)
            {
                Console.WriteLine("No messages entered. Test cancelled.");
                WaitForKeyPress();
                return;
            }
            
            string historyText = string.Join("\n", messageHistory.Select((msg, i) => $"Message {i+1}: {msg}"));
            
            string prompt = $"You are an AI moderator analyzing chat history for patterns of inappropriate content. " +
                            $"Review the following message history from a single user in a family-friendly chat application:\n\n" +
                            $"{historyText}\n\n" +
                            $"Return a JSON response with:\n" +
                            $"1. Overall assessment (safe, mild concerns, serious concerns)\n" +
                            $"2. Pattern detection (is there an escalating pattern of inappropriate content?)\n" +
                            $"3. Individual message analysis (for each message: inappropriate or not, severity)\n" +
                            $"4. Recommended moderation action based on the pattern\n" +
                            $"5. Explanation of the analysis";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }
    }
} 