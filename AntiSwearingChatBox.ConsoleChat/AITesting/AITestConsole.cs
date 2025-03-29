using AntiSwearingChatBox.AI;
using AntiSwearingChatBox.AI.Moderation;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.AITesting
{
    public class AITestConsole
    {
        private readonly GeminiService _geminiService;
        private readonly ModelSettings _modelSettings;

        public AITestConsole(GeminiService geminiService, ModelSettings modelSettings)
        {
            _geminiService = geminiService;
            _modelSettings = modelSettings;
        }

        public async Task RunConsoleAsync()
        {
            Console.WriteLine("=== Anti-Swearing Chat Box API Test Console ===");
            Console.WriteLine("This application tests the Gemini AI integration directly.");
            Console.WriteLine("All responses are in JSON format and use configurable settings.");
            Console.WriteLine();
            
            // Display moderation settings
            DisplayModerationSettings();

            bool exit = false;
            while (!exit)
            {
                try
                {
                    DisplayMainMenu();
                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await new TextGenerationTest(_geminiService).RunAsync();
                            break;
                        case "2":
                            await new MessageModerationTest(_geminiService).RunAsync();
                            break;
                        case "3":
                            await new ProfanityDetectionTest(_geminiService).RunAsync();
                            break;
                        case "4":
                            await new ContextAwareFilteringTest(_geminiService).RunAsync();
                            break;
                        case "5":
                            await new SentimentAnalysisTest(_geminiService).RunAsync();
                            break;
                        case "6":
                            await new AutoResponsesTest(_geminiService).RunAsync();
                            break;
                        case "7":
                            await new MessageHistoryReviewTest(_geminiService).RunAsync();
                            break;
                        case "8":
                            await new AlternativeSuggestionTest(_geminiService).RunAsync();
                            break;
                        case "9":
                            await new LanguageSpecificModerationTest(_geminiService).RunAsync();
                            break;
                        case "10":
                            await new ReputationAnalysisTest(_geminiService).RunAsync();
                            break;
                        case "11":
                            DisplayDetailedModelSettings();
                            break;
                        case "0":
                            Console.WriteLine("Exiting application...");
                            exit = true;
                            break;
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

        private void DisplayMainMenu()
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
        }

        private void DisplayModerationSettings()
        {
            Console.WriteLine("\n=== Moderation Settings Loaded ===");
            Console.WriteLine($"Default Language: {_modelSettings.Moderation.DefaultLanguage}");
            Console.WriteLine($"Sensitivity Level: {_modelSettings.Moderation.Sensitivity}");
            Console.WriteLine($"Number of Filtering Rules: {_modelSettings.Moderation.FilteringRules.Count}");
            Console.WriteLine($"Preserve Original Text: {_modelSettings.Moderation.ResponseOptions.PreserveOriginalText}");
            Console.WriteLine($"Include Explanations: {_modelSettings.Moderation.ResponseOptions.IncludeExplanations}");
        }

        private void DisplayDetailedModelSettings()
        {
            Console.WriteLine("\n=== Detailed Model Settings ===");
            
            // Main settings
            Console.WriteLine($"Default Language: {_modelSettings.Moderation.DefaultLanguage}");
            Console.WriteLine($"Sensitivity: {_modelSettings.Moderation.Sensitivity}");
            Console.WriteLine($"Languages Always Moderated: {string.Join(", ", _modelSettings.Moderation.AlwaysModerateLanguages)}");
            
            // Display filtering rules
            Console.WriteLine("\nFiltering Rules:");
            foreach (var rule in _modelSettings.Moderation.FilteringRules)
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
            Console.WriteLine($"  Include Explanations: {_modelSettings.Moderation.ResponseOptions.IncludeExplanations}");
            Console.WriteLine($"  Strict JSON Format: {_modelSettings.Moderation.ResponseOptions.StrictJsonFormat}");
            Console.WriteLine($"  Preserve Original Text: {_modelSettings.Moderation.ResponseOptions.PreserveOriginalText}");
            Console.WriteLine($"  Show Confidence Scores: {_modelSettings.Moderation.ResponseOptions.ShowConfidenceScores}");
            Console.WriteLine($"  Always Show Cultural Context: {_modelSettings.Moderation.ResponseOptions.AlwaysShowCulturalContext}");
            
            // AI Instructions
            Console.WriteLine("\nAI Instructions:");
            Console.WriteLine($"  Prompt Prefix: {_modelSettings.Moderation.AIInstructions.PromptPrefix}");
            Console.WriteLine("  Rules:");
            foreach (var rule in _modelSettings.Moderation.AIInstructions.Rules)
            {
                Console.WriteLine($"    - {rule}");
            }
            
            // Warning thresholds
            Console.WriteLine("\nWarning Thresholds:");
            Console.WriteLine($"  Low Warning Count: {_modelSettings.Moderation.WarningThresholds.LowWarningCount}");
            Console.WriteLine($"  Medium Warning Count: {_modelSettings.Moderation.WarningThresholds.MediumWarningCount}");
            Console.WriteLine($"  High Warning Count: {_modelSettings.Moderation.WarningThresholds.HighWarningCount}");
            Console.WriteLine($"  Warning Expiration: {_modelSettings.Moderation.WarningThresholds.WarningExpiration}");
            
            WaitForKeyPress();
        }

        private void WaitForKeyPress()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static void FormatAndPrintJson(string json)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var element = JsonSerializer.Deserialize<JsonElement>(json);
                var formattedJson = JsonSerializer.Serialize(element, options);
                Console.WriteLine(formattedJson);
            }
            catch
            {
                Console.WriteLine(json);
            }
        }
    }
} 