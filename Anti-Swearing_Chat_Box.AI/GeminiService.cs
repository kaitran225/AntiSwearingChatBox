using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace Anti_Swearing_Chat_Box.AI
{
    public class GeminiService
    {
        private readonly GenerativeModel _model;
        private readonly GeminiSettings _settings;

        public GeminiService(IOptions<GeminiSettings> options)
        {
            _settings = options.Value;
            var googleAI = new GoogleAI(apiKey: _settings.ApiKey);
            _model = googleAI.GenerativeModel(model: _settings.ModelName);
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            try
            {
                var config = new GenerationConfig
                {
                    Temperature = 0.7f,
                    MaxOutputTokens = 1024
                };

                var response = await _model.GenerateContent(prompt, config);
                return response.Text ?? "No response generated";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Modified to return JSON
        public async Task<string> GenerateJsonResponseAsync(string prompt)
        {
            try
            {
                string jsonPrompt = $"Respond to the following request in valid JSON format only: {prompt}";
                
                var config = new GenerationConfig
                {
                    Temperature = 0.7f,
                    MaxOutputTokens = 1024
                };

                var response = await _model.GenerateContent(jsonPrompt, config);
                string responseText = response.Text ?? "{}";
                
                // Ensure response is valid JSON
                try 
                {
                    // Attempt to parse as JSON to validate
                    JsonDocument.Parse(responseText);
                    return responseText;
                }
                catch
                {
                    // If not valid JSON, create a simple valid JSON object
                    return JsonSerializer.Serialize(new { text = responseText });
                }
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        public async Task<string> ModerateChatMessageAsync(string message)
        {
            string prompt = $"Review the following message and determine if it contains swear words or inappropriate language. " +
                            $"If it does, replace those words with appropriate alternatives or censorship. " +
                            $"Return the result in JSON format with the following structure: {{\"original\": \"original message\", \"moderated\": \"moderated message\", \"wasModified\": true/false}}. " +
                            $"Original message: {message}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 1. Profanity Detection & Warning
        public async Task<string> DetectProfanityAsync(string message)
        {
            string prompt = $"Review the following message and determine if it contains swear words or inappropriate language. " +
                            $"If it does, identify the specific inappropriate words or phrases and explain why they might be offensive. " +
                            $"Respond only in JSON format with the following structure: " +
                            $"{{\"containsProfanity\": true/false, \"inappropriateTerms\": [\"word1\", \"word2\"], " +
                            $"\"explanation\": \"explanation of why these terms are inappropriate\", " +
                            $"\"originalMessage\": \"{message}\"}} " +
                            $"If no inappropriate language is found, set containsProfanity to false and leave the inappropriateTerms array empty. " +
                            $"Message: {message}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 2. Context-Aware Filtering
        public async Task<string> PerformContextAwareFilteringAsync(string message, string conversationContext)
        {
            string prompt = $"Review the following message in the context of the conversation. " +
                           $"Determine if it contains inappropriate language considering the full context (sarcasm, cultural references, dual meanings). " +
                           $"Return only a JSON object with the structure: " +
                           $"{{\"originalMessage\": \"{message}\", \"moderatedMessage\": \"modified version here\", " +
                           $"\"wasModified\": true/false, \"contextualExplanation\": \"explanation about the context-aware decision\"}} " +
                           $"Conversation context: {conversationContext} " +
                           $"Message to review: {message}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 3. Sentiment Analysis & Toxicity Detection
        public async Task<string> AnalyzeSentimentAsync(string message)
        {
            string prompt = $"Analyze the sentiment and toxicity of the following message and return ONLY a JSON response. " +
                           $"Include the following keys: sentimentScore (1-10, 10 being most positive), " +
                           $"toxicityLevel (none, low, medium, high), emotions (array of emotions detected), " +
                           $"requiresIntervention (boolean), interventionReason (string), and analysis (brief explanation). " +
                           $"Message: {message}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 4. AI-Powered Auto-Responses
        public async Task<string> GenerateDeescalationResponseAsync(string harmfulMessage)
        {
            string prompt = $"A user has received the following potentially harmful message: \"{harmfulMessage}\" " +
                           $"Generate a thoughtful, de-escalating response that helps resolve conflict. " +
                           $"Return ONLY a JSON object with the structure: {{\"harmfulMessage\": \"original harmful message\", " +
                           $"\"deescalationResponse\": \"your response here\", " +
                           $"\"responseStrategy\": \"brief explanation of the strategy used\"}}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 5. Message History Review
        public async Task<string> ReviewMessageHistoryAsync(List<string> messageHistory)
        {
            string messagesFormatted = string.Join("\n", messageHistory);
            
            string prompt = $"Review the following message history and identify any patterns of inappropriate language, " +
                           $"harassment, or concerning behavior. Return ONLY a JSON object with the following structure: " +
                           $"{{\"messageCount\": number, \"flaggedMessages\": [{{\"index\": 0, \"content\": \"message\", \"reason\": \"reason flagged\"}}], " +
                           $"\"overallAssessment\": \"conversation health summary\", " +
                           $"\"recommendedActions\": [\"action1\", \"action2\"]}} " +
                           $"Message history: \n{messagesFormatted}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 6. AI-Powered Moderation Dashboard Data
        public async Task<string> GenerateModerationInsightsAsync(List<string> flaggedMessages, List<string> userWarnings)
        {
            string flaggedMessagesFormatted = string.Join("\n", flaggedMessages);
            string userWarningsFormatted = string.Join("\n", userWarnings);
            
            string prompt = $"Analyze the following set of flagged messages and user warnings to identify trends. " +
                           $"Return ONLY a JSON object with keys: commonViolations (array), emergingTerms (array), " +
                           $"moderationEffectiveness (percentage), recommendations (array), and insightSummary (string). " +
                           $"Flagged messages: \n{flaggedMessagesFormatted}\n\n" +
                           $"User warnings: \n{userWarningsFormatted}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 7. AI-Based Alternative Suggestion
        public async Task<string> SuggestAlternativeMessageAsync(string inappropriateMessage)
        {
            string prompt = $"The following message contains inappropriate language or tone: \"{inappropriateMessage}\" " +
                           $"Suggest a more constructive and polite way to express the same idea. " +
                           $"Return ONLY a JSON object with the structure: " +
                           $"{{\"originalMessage\": \"original message\", \"suggestedAlternative\": \"suggested rewrite\", " +
                           $"\"explanation\": \"why this alternative is better\"}}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 8. Language-Specific Moderation
        public async Task<string> ModerateMultiLanguageMessageAsync(string message, string detectedLanguage)
        {
            string prompt = $"Moderate the following message which is in {detectedLanguage}. " +
                           $"Identify and replace any inappropriate language specific to this language and culture. " +
                           $"Return ONLY a JSON object with the structure: " +
                           $"{{\"originalMessage\": \"original message\", \"moderatedMessage\": \"moderated message\", " +
                           $"\"language\": \"{detectedLanguage}\", \"wasModified\": true/false, " +
                           $"\"culturalContext\": \"any important cultural notes\"}}";
            
            return await GenerateJsonResponseAsync(prompt);
        }

        // 9. Reputation & Trust Score Analysis
        public async Task<string> AnalyzeUserReputationAsync(List<string> userMessages, int priorWarnings)
        {
            string messagesFormatted = string.Join("\n", userMessages);
            
            string prompt = $"Analyze the following message history from a user with {priorWarnings} prior warnings. " +
                           $"Return ONLY a JSON object with reputationScore (1-100), behaviorPatterns (array), " +
                           $"trustworthiness (low/medium/high), recommendedActions (array), and analysis (string). " +
                           $"Message history: \n{messagesFormatted}";
            
            return await GenerateJsonResponseAsync(prompt);
        }
    }

    // Result classes for the new functions - kept for backward compatibility
    public class ProfanityDetectionResult
    {
        public bool ContainsProfanity { get; set; }
        public string OriginalMessage { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
    }

    public class ContextualModerationResult
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string ModeratedMessage { get; set; } = string.Empty;
        public bool WasModified { get; set; }
    }

    public class SentimentAnalysisResult
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string FullAnalysis { get; set; } = string.Empty;
        public bool RequiresIntervention { get; set; }
    }

    public class MessageReviewSummary
    {
        public string ReviewSummary { get; set; } = string.Empty;
        public int MessageCount { get; set; }
    }

    public class ModerationDashboardData
    {
        public string TrendAnalysis { get; set; } = string.Empty;
        public int FlaggedMessageCount { get; set; }
        public int UserWarningCount { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class AlternativeMessageSuggestion
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string SuggestedAlternative { get; set; } = string.Empty;
    }

    public class MultiLanguageModerationResult
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string ModeratedMessage { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool WasModified { get; set; }
    }

    public class UserReputationAnalysis
    {
        public string AnalysisResults { get; set; } = string.Empty;
        public int ReputationScore { get; set; }
        public int PriorWarningCount { get; set; }
        public int MessagesSampled { get; set; }
    }
} 