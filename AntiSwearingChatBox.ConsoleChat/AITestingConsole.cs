using AntiSwearingChatBox.AI;
using AntiSwearingChatBox.AI.Moderation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AntiSwearingChatBox.ConsoleChat
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Anti-Swearing Chat Box API Test Console ===");
            Console.WriteLine("This application tests the Gemini AI integration directly.");
            Console.WriteLine("All responses are in JSON format and use configurable settings.");
            Console.WriteLine();

            // Load configuration from appsettings.json
            string projectDir = Directory.GetCurrentDirectory();
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Initialize Gemini Service with settings from configuration
            var settings = new GeminiSettings
            {
                ApiKey = configuration["GeminiSettings:ApiKey"] ?? "AIzaSyD9Odq-PkFqA2HHYsR86EEhPbM85eHF2Sw",
                ModelName = configuration["GeminiSettings:ModelName"] ?? "gemini-1.5-pro"
            };
            
            Console.WriteLine($"Using model: {settings.ModelName}");
            Console.WriteLine($"API Key: {settings.ApiKey.Substring(0, 8)}...[redacted]");
            
            // Load and display moderation settings
            var modelSettings = ModelSettings.Instance;
            Console.WriteLine("\n=== Moderation Settings Loaded ===");
            Console.WriteLine($"Default Language: {modelSettings.Moderation.DefaultLanguage}");
            Console.WriteLine($"Sensitivity Level: {modelSettings.Moderation.Sensitivity}");
            Console.WriteLine($"Number of Filtering Rules: {modelSettings.Moderation.FilteringRules.Count}");
            Console.WriteLine($"Preserve Original Text: {modelSettings.Moderation.ResponseOptions.PreserveOriginalText}");
            Console.WriteLine($"Include Explanations: {modelSettings.Moderation.ResponseOptions.IncludeExplanations}");
            
            var geminiService = new GeminiService(Options.Create(settings));

            while (true)
            {
                Console.WriteLine("\n=== Main Menu ===");
                Console.WriteLine("1. Test Text Generation");
                Console.WriteLine("2. Basic Message Moderation");
                Console.WriteLine("3. Profanity Detection & Warning");
                Console.WriteLine("4. Context-Aware Filtering");
                Console.WriteLine("5. Sentiment Analysis & Toxicity Detection");
                Console.WriteLine("6. AI-Powered Auto-Responses");
                Console.WriteLine("7. Message History Review");
                Console.WriteLine("8. AI-Based Alternative Suggestion");
                Console.WriteLine("9. Language-Specific Moderation");
                Console.WriteLine("10. Reputation & Trust Score Analysis");
                Console.WriteLine("11. Display Current Model Settings");
                Console.WriteLine("0. Exit");
                Console.Write("\nYour choice: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await TestTextGeneration(geminiService);
                            break;
                        case "2":
                            await TestMessageModeration(geminiService);
                            break;
                        case "3":
                            await TestProfanityDetection(geminiService);
                            break;
                        case "4":
                            await TestContextAwareFiltering(geminiService);
                            break;
                        case "5":
                            await TestSentimentAnalysis(geminiService);
                            break;
                        case "6":
                            await TestAutoResponses(geminiService);
                            break;
                        case "7":
                            await TestMessageHistoryReview(geminiService);
                            break;
                        case "8":
                            await TestAlternativeSuggestion(geminiService);
                            break;
                        case "9":
                            await TestLanguageSpecificModeration(geminiService);
                            break;
                        case "10":
                            await TestReputationAnalysis(geminiService);
                            break;
                        case "11":
                            DisplayModelSettings();
                            break;
                        case "0":
                            Console.WriteLine("Exiting application...");
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        // Display detailed model settings
        static void DisplayModelSettings()
        {
            var settings = ModelSettings.Instance;
            
            Console.WriteLine("\n=== Detailed Model Settings ===");
            
            // Main settings
            Console.WriteLine($"Default Language: {settings.Moderation.DefaultLanguage}");
            Console.WriteLine($"Sensitivity: {settings.Moderation.Sensitivity}");
            Console.WriteLine($"Languages Always Moderated: {string.Join(", ", settings.Moderation.AlwaysModerateLanguages)}");
            
            // Display filtering rules
            Console.WriteLine("\nFiltering Rules:");
            foreach (var rule in settings.Moderation.FilteringRules)
            {
                Console.WriteLine($"  - {rule.RuleType} (Enabled: {rule.Enabled})");
                Console.WriteLine($"    Sensitivity Level: {rule.SensitivityLevel}");
                
                if (rule.RuleType == "ProfanityFilter")
                {
                    Console.WriteLine($"    Allowed Exceptions: {string.Join(", ", rule.AllowedExceptions)}");
                    Console.WriteLine($"    Always Filter: {string.Join(", ", rule.AlwaysFilterTerms)}");
                }
                else if (rule.RuleType == "ToxicityFilter")
                {
                    Console.WriteLine($"    Detect Hate Speech: {rule.DetectHateSpeech}");
                    Console.WriteLine($"    Detect Threats: {rule.DetectThreats}");
                    Console.WriteLine($"    Detect Sexual Content: {rule.DetectSexualContent}");
                }
                else if (rule.RuleType == "ContextAwareFilter")
                {
                    Console.WriteLine($"    Consider Conversation History: {rule.ConsiderConversationHistory}");
                    Console.WriteLine($"    Detect Sarcasm: {rule.DetectSarcasm}");
                    Console.WriteLine($"    Detect Humor: {rule.DetectHumor}");
                }
            }
            
            // Response options
            Console.WriteLine("\nResponse Options:");
            Console.WriteLine($"  Include Explanations: {settings.Moderation.ResponseOptions.IncludeExplanations}");
            Console.WriteLine($"  Strict JSON Format: {settings.Moderation.ResponseOptions.StrictJsonFormat}");
            Console.WriteLine($"  Preserve Original Text: {settings.Moderation.ResponseOptions.PreserveOriginalText}");
            Console.WriteLine($"  Show Confidence Scores: {settings.Moderation.ResponseOptions.ShowConfidenceScores}");
            Console.WriteLine($"  Always Show Cultural Context: {settings.Moderation.ResponseOptions.AlwaysShowCulturalContext}");
            
            // AI Instructions
            Console.WriteLine("\nAI Instructions:");
            Console.WriteLine($"  Prompt Prefix: {settings.Moderation.AIInstructions.PromptPrefix}");
            Console.WriteLine("  Rules:");
            foreach (var rule in settings.Moderation.AIInstructions.Rules)
            {
                Console.WriteLine($"    - {rule}");
            }
            
            // Warning thresholds
            Console.WriteLine("\nWarning Thresholds:");
            Console.WriteLine($"  Low Warning Count: {settings.Moderation.WarningThresholds.LowWarningCount}");
            Console.WriteLine($"  Medium Warning Count: {settings.Moderation.WarningThresholds.MediumWarningCount}");
            Console.WriteLine($"  High Warning Count: {settings.Moderation.WarningThresholds.HighWarningCount}");
            Console.WriteLine($"  Warning Expiration: {settings.Moderation.WarningThresholds.WarningExpiration}");
            
            WaitForKeyPress();
        }

        static async Task TestTextGeneration(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Text Generation Test ===");
            Console.Write("Enter a prompt: ");
            string prompt = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nGenerating response...");
            
            string response = await geminiService.GenerateJsonResponseAsync(prompt);
            Console.WriteLine("\nJSON Response:");
            FormatAndPrintJson(response);
            
            WaitForKeyPress();
        }

        static async Task TestMessageModeration(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Basic Message Moderation Test ===");
            Console.WriteLine("Enter a message to moderate (try including inappropriate language to test moderation):");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nModerating message...");
            
            string jsonResponse = await geminiService.ModerateChatMessageAsync(message);
            
            Console.WriteLine("\nModeration Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestProfanityDetection(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Profanity Detection & Warning Test ===");
            Console.WriteLine("Enter a message to check for profanity:");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nDetecting profanity...");
            
            string jsonResponse = await geminiService.DetectProfanityAsync(message);
            
            Console.WriteLine("\nProfanity Detection Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestContextAwareFiltering(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Context-Aware Filtering Test ===");
            Console.WriteLine("Enter the conversation context:");
            string context = Console.ReadLine() ?? string.Empty;
            Console.WriteLine("Enter the message to filter:");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nPerforming context-aware filtering...");
            
            string jsonResponse = await geminiService.PerformContextAwareFilteringAsync(message, context);
            
            Console.WriteLine("\nContext-Aware Filtering Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestSentimentAnalysis(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Sentiment Analysis & Toxicity Test ===");
            Console.WriteLine("Enter a message to analyze for sentiment and toxicity:");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nAnalyzing sentiment and toxicity...");
            
            string jsonResponse = await geminiService.AnalyzeSentimentAsync(message);
            
            Console.WriteLine("\nSentiment Analysis Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestAutoResponses(GeminiService geminiService)
        {
            Console.WriteLine("\n=== AI-Powered Auto-Response Test ===");
            Console.WriteLine("Enter a potentially harmful message to generate a de-escalation response:");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nGenerating de-escalation response...");
            
            string jsonResponse = await geminiService.GenerateDeescalationResponseAsync(message);
            
            Console.WriteLine("\nAuto-Response Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestMessageHistoryReview(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Message History Review Test ===");
            Console.WriteLine("Enter messages for history review (type 'done' when finished):");
            
            var messages = new List<string>();
            while (true)
            {
                Console.Write("> ");
                string message = Console.ReadLine() ?? string.Empty;
                if (message.ToLower() == "done") break;
                messages.Add(message);
            }

            if (messages.Count == 0)
            {
                Console.WriteLine("No messages entered.");
                WaitForKeyPress();
                return;
            }

            Console.WriteLine("\nReviewing message history...");
            
            string jsonResponse = await geminiService.ReviewMessageHistoryAsync(messages);
            
            Console.WriteLine("\nMessage History Review Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestAlternativeSuggestion(GeminiService geminiService)
        {
            Console.WriteLine("\n=== AI-Based Alternative Suggestion Test ===");
            Console.WriteLine("Enter an inappropriate message to get a better alternative:");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nGenerating alternative suggestion...");
            
            string jsonResponse = await geminiService.SuggestAlternativeMessageAsync(message);
            
            Console.WriteLine("\nAlternative Suggestion Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestLanguageSpecificModeration(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Language-Specific Moderation Test ===");
            Console.WriteLine("Enter a language (e.g., Spanish, French, German):");
            string language = Console.ReadLine() ?? "English";
            Console.WriteLine($"Enter a message in {language} to moderate:");
            string message = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("\nModerating multi-language message...");
            
            string jsonResponse = await geminiService.ModerateMultiLanguageMessageAsync(message, language);
            
            Console.WriteLine("\nLanguage-Specific Moderation Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static async Task TestReputationAnalysis(GeminiService geminiService)
        {
            Console.WriteLine("\n=== Reputation & Trust Score Analysis Test ===");
            Console.WriteLine("Enter number of prior warnings for this user:");
            if (!int.TryParse(Console.ReadLine(), out int priorWarnings))
            {
                priorWarnings = 0;
            }
            
            Console.WriteLine("Enter user's message history (type 'done' when finished):");
            
            var messages = new List<string>();
            while (true)
            {
                Console.Write("> ");
                string message = Console.ReadLine() ?? string.Empty;
                if (message.ToLower() == "done") break;
                messages.Add(message);
            }

            if (messages.Count == 0)
            {
                Console.WriteLine("No messages entered.");
                WaitForKeyPress();
                return;
            }

            Console.WriteLine("\nAnalyzing user reputation...");
            
            string jsonResponse = await geminiService.AnalyzeUserReputationAsync(messages, priorWarnings);
            
            Console.WriteLine("\nReputation Analysis Results (JSON):");
            FormatAndPrintJson(jsonResponse);
            
            WaitForKeyPress();
        }

        static void FormatAndPrintJson(string jsonString)
        {
            try
            {
                // Parse the JSON for processing
                using var jsonDocument = JsonDocument.Parse(jsonString);
                var root = jsonDocument.RootElement;
                
                // Process and display key information in a user-friendly format
                Console.WriteLine("\n=== PROCESSED RESULTS ===");
                ProcessJsonByKeys(root);
                
                // Display the full formatted JSON for reference
                Console.WriteLine("\n=== FULL JSON RESPONSE ===");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string formattedJson = JsonSerializer.Serialize(jsonDocument, options);
                Console.WriteLine(formattedJson);
            }
            catch (Exception ex)
            {
                // If parsing fails, just print the raw string
                Console.WriteLine("Failed to process JSON: " + ex.Message);
                Console.WriteLine(jsonString);
            }
        }
        
        static void ProcessJsonByKeys(JsonElement element)
        {
            // Check for common keys and display their values in a readable format
            foreach (var property in element.EnumerateObject())
            {
                switch (property.Name.ToLower())
                {
                    // Moderation results
                    case "wasmodified":
                        Console.WriteLine($"Content was modified: {property.Value}");
                        break;
                    case "original":
                    case "originalmessage":
                        Console.WriteLine($"Original message: \"{property.Value}\"");
                        break;
                    case "moderated":
                    case "moderatedmessage":
                        Console.WriteLine($"Moderated message: \"{property.Value}\"");
                        break;
                    
                    // Profanity detection
                    case "containsprofanity":
                        Console.WriteLine($"Contains profanity: {property.Value}");
                        break;
                    case "inappropriateterms":
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            Console.WriteLine("Inappropriate terms detected:");
                            foreach (var term in property.Value.EnumerateArray())
                            {
                                Console.WriteLine($"  - {term}");
                            }
                        }
                        break;
                    case "explanation":
                        Console.WriteLine($"Explanation: {property.Value}");
                        break;
                    
                    // Sentiment analysis
                    case "sentimentscore":
                        Console.WriteLine($"Sentiment score: {property.Value}/10");
                        break;
                    case "toxicitylevel":
                        Console.WriteLine($"Toxicity level: {property.Value}");
                        break;
                    case "emotions":
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            Console.WriteLine("Emotions detected:");
                            foreach (var emotion in property.Value.EnumerateArray())
                            {
                                Console.WriteLine($"  - {emotion}");
                            }
                        }
                        break;
                    case "requiresintervention":
                        Console.WriteLine($"Requires intervention: {property.Value}");
                        break;
                    
                    // Deescalation responses
                    case "deescalationresponse":
                        Console.WriteLine($"De-escalation response: \"{property.Value}\"");
                        break;
                    case "responsestrategy":
                        Console.WriteLine($"Response strategy: {property.Value}");
                        break;
                    
                    // Alternative suggestions
                    case "suggestedalternative":
                        Console.WriteLine($"Suggested alternative: \"{property.Value}\"");
                        break;
                    
                    // Context information
                    case "contextualexplanation":
                        Console.WriteLine($"Contextual explanation: {property.Value}");
                        break;
                    case "culturalcontext":
                        Console.WriteLine($"Cultural context: {property.Value}");
                        break;
                    
                    // Message history review
                    case "messagecount":
                        Console.WriteLine($"Messages analyzed: {property.Value}");
                        break;
                    case "overallassessment":
                        Console.WriteLine($"Overall assessment: {property.Value}");
                        break;
                    case "flaggedmessages":
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            Console.WriteLine("Flagged messages:");
                            int count = 0;
                            foreach (var msg in property.Value.EnumerateArray())
                            {
                                count++;
                                string content = "n/a";
                                string reason = "n/a";
                                
                                if (msg.TryGetProperty("content", out var contentElement))
                                    content = contentElement.ToString();
                                
                                if (msg.TryGetProperty("reason", out var reasonElement))
                                    reason = reasonElement.ToString();
                                
                                Console.WriteLine($"  {count}. \"{content}\" - {reason}");
                            }
                        }
                        break;
                    
                    // Reputation analysis
                    case "reputationscore":
                        Console.WriteLine($"Reputation score: {property.Value}/100");
                        break;
                    case "trustworthiness":
                        Console.WriteLine($"Trustworthiness: {property.Value}");
                        break;
                    case "behaviorpatterns":
                    case "recommendedactions":
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            Console.WriteLine($"{FormatCamelCase(property.Name)}:");
                            int count = 0;
                            foreach (var item in property.Value.EnumerateArray())
                            {
                                count++;
                                Console.WriteLine($"  {count}. {item}");
                            }
                        }
                        break;
                    
                    // Moderation insights
                    case "commonviolations":
                    case "emergingterms":
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            Console.WriteLine($"{FormatCamelCase(property.Name)}:");
                            foreach (var item in property.Value.EnumerateArray())
                            {
                                Console.WriteLine($"  - {item}");
                            }
                        }
                        break;
                    case "moderationeffectiveness":
                        Console.WriteLine($"Moderation effectiveness: {property.Value}");
                        break;
                    case "insightsummary":
                        Console.WriteLine($"Insight summary: {property.Value}");
                        break;
                    
                    // General purpose text
                    case "text":
                        Console.WriteLine($"Text: {property.Value}");
                        break;
                    case "analysis":
                        Console.WriteLine($"Analysis: {property.Value}");
                        break;
                    
                    // Error handling
                    case "error":
                        Console.WriteLine($"ERROR: {property.Value}");
                        break;
                }
            }
        }
        
        static string FormatCamelCase(string text)
        {
            // Convert camelCase to Title Case with spaces
            if (string.IsNullOrEmpty(text))
                return text;
                
            var result = "";
            result += char.ToUpper(text[0]);
            
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    result += " " + text[i];
                else
                    result += text[i];
            }
            
            return result;
        }

        static void WaitForKeyPress()
        {
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }
}
