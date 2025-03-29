using AntiSwearingChatBox.AI;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class ReputationAnalysisTest : TestBase
    {
        public ReputationAnalysisTest(GeminiService geminiService) : base(geminiService)
        {
        }

        public override async Task RunAsync()
        {
            PrintTitle("Reputation & Trust Score Analysis Test");
            Console.WriteLine("This test analyzes a user's message history to calculate a reputation score.");
            
            Console.Write("Enter a username: ");
            string username = Console.ReadLine() ?? "User1";
            
            var messageHistory = new List<string>();
            Console.WriteLine("\nEnter up to 5 messages from this user's history (one per line).");
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
            
            Console.WriteLine("\nEnter any warnings or moderation actions against this user (one per line).");
            Console.WriteLine("Format: [action type] - [reason] - [date]");
            Console.WriteLine("Examples: 'warning - inappropriate language - 2023-06-15'");
            Console.WriteLine("Press Enter on an empty line when finished.");
            
            var moderationHistory = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                Console.Write($"Action {i+1}: ");
                string action = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(action))
                    break;
                    
                moderationHistory.Add(action);
            }
            
            string historyText = string.Join("\n", messageHistory.Select((msg, i) => $"Message {i+1}: {msg}"));
            string moderationText = moderationHistory.Count > 0 
                ? string.Join("\n", moderationHistory.Select((act, i) => $"Action {i+1}: {act}"))
                : "No prior moderation actions";
            
            string prompt = $"You are an AI analyzing a user's reputation and trust score in a family-friendly chat application. " +
                            $"Review the following user's message history and moderation history to calculate a trust score and make recommendations.\n\n" +
                            $"Username: {username}\n\n" +
                            $"Message history:\n{historyText}\n\n" +
                            $"Moderation history:\n{moderationText}\n\n" +
                            $"Return a JSON response with:\n" +
                            $"1. Reputation score (0-100 scale)\n" +
                            $"2. Trust level category (high, medium, low, probation)\n" +
                            $"3. Analysis of communication patterns\n" +
                            $"4. Risk assessment for future violations\n" +
                            $"5. Recommendation for moderators (more monitoring, standard monitoring, reduced restrictions, etc.)\n" +
                            $"6. Explanation of the analysis and score";

            await ProcessAndPrintResponse(prompt);
            WaitForKeyPress();
        }
    }
} 