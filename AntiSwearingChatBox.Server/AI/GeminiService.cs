using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace AntiSwearingChatBox.AI
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
            
            System.Console.WriteLine($"GeminiService initialized with model: {_settings.ModelName}");
            System.Diagnostics.Debug.WriteLine($"GeminiService initialized with model: {_settings.ModelName}");
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
                System.Console.WriteLine($"GenerateTextAsync error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

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
                
                try 
                {
                    JsonDocument.Parse(responseText);
                    return responseText;
                }
                catch
                {
                    return JsonSerializer.Serialize(new { text = responseText });
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"GenerateJsonResponseAsync error: {ex.Message}");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        public async Task<string> ModerateChatMessageAsync(string message)
        {
            string promptTemplate = 
                $"CRITICAL MODERATION TASK: Analyze the following message for ANY type of profanity, swear words, or inappropriate language.\n\n" +
                $"You must detect profanity even if it uses letter substitutions, character omissions, or unusual spellings. " +
                $"Examples of variations to catch:\n" +
                $"- 'fuck', 'fuk', 'fvck', 'fuuck', 'f*ck', 'f**k', 'fck'\n" +
                $"- 'shit', 'sh*t', 'sh1t', 'sht', 'shiit'\n" +
                $"- 'ass', 'a$$', 'a**', '@ss'\n\n" +
                $"Err on the side of caution. If something might be profanity, treat it as profanity.\n\n" +
                $"Return the result in JSON format with the following structure:\n" +
                $"{{\"original\": \"original message\", \"moderated\": \"moderated message with all profanity replaced by asterisks\", \"wasModified\": true/false}}\n\n" +
                $"MESSAGE TO MODERATE: \"{message}\"";
            
            System.Console.WriteLine($"Sending message for moderation: \"{message}\"");
            return await RequestProcessor.ProcessModeration(this, message, promptTemplate);
        }

        /// <summary>
        /// Detects profanity and inappropriate language in a message
        /// </summary>
        public async Task<string> DetectProfanityAsync(string message)
        {
            try
            {
                Console.WriteLine($"Checking message for profanity: \"{message}\"");
                
                // Create an enhanced prompt that specifically targets common evasion techniques
                string enhancedPrompt = RequestProcessor.EnhancePrompt(message, "profanity");
                
                // Call Gemini with the enhanced prompt
                var response = await GenerateJsonResponseAsync(enhancedPrompt);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateJsonResponseAsync error: {ex.Message}");
                return $"{{\"error\":\"{ex.Message}\"}}";
            }
        }

        public async Task<string> PerformContextAwareFilteringAsync(string message, string conversationContext)
        {
            string promptTemplate = 
                $"Review the following message in the context of the conversation. " +
                $"Determine if it contains inappropriate language considering the full context (sarcasm, cultural references, dual meanings). " +
                $"Return only a JSON object with the structure: " +
                $"{{\"originalMessage\": \"original message here\", \"moderatedMessage\": \"modified version here\", " +
                $"\"wasModified\": true/false, \"contextualExplanation\": \"explanation about the context-aware decision\"}} " +
                $"Conversation context: {conversationContext}";
            
            return await RequestProcessor.ProcessModeration(this, message, promptTemplate);
        }

        public async Task<string> AnalyzeSentimentAsync(string message)
        {
            string promptTemplate = $"Analyze the sentiment and toxicity of the following message and return ONLY a JSON response. " +
                           $"Include the following keys: sentimentScore (1-10, 10 being most positive), " +
                           $"toxicityLevel (none, low, medium, high), emotions (array of emotions detected), " +
                           $"requiresIntervention (boolean), interventionReason (string), and analysis (brief explanation).";
            
            return await RequestProcessor.ProcessModeration(this, message, promptTemplate);
        }

        public async Task<string> GenerateDeescalationResponseAsync(string harmfulMessage)
        {
            string promptTemplate = $"A user has received the following potentially harmful message. " +
                           $"Generate a thoughtful, de-escalating response that helps resolve conflict. " +
                           $"Return ONLY a JSON object with the structure: {{\"harmfulMessage\": \"original message here\", " +
                           $"\"deescalationResponse\": \"your response here\", " +
                           $"\"responseStrategy\": \"brief explanation of the strategy used\"}}";
            
            return await RequestProcessor.ProcessModeration(this, harmfulMessage, promptTemplate);
        }
        public async Task<string> ReviewMessageHistoryAsync(List<string> messageHistory)
        {
            string messagesFormatted = string.Join("\n", messageHistory);
            
            string promptTemplate = $"Review the following message history and identify any patterns of inappropriate language, " +
                           $"harassment, or concerning behavior. Return ONLY a JSON object with the following structure: " +
                           $"{{\"messageCount\": number, \"flaggedMessages\": [{{\"index\": 0, \"content\": \"message\", \"reason\": \"reason flagged\"}}], " +
                           $"\"overallAssessment\": \"conversation health summary\", " +
                           $"\"recommendedActions\": [\"action1\", \"action2\"]}} " +
                           $"Message history: \n{messagesFormatted}";
            
            return await GenerateJsonResponseAsync(promptTemplate);
        }

        public async Task<string> SuggestAlternativeMessageAsync(string inappropriateMessage)
        {
            string promptTemplate = $"The following message contains inappropriate language or tone. " +
                           $"Suggest a more constructive and polite way to express the same idea. " +
                           $"Return ONLY a JSON object with the structure: " +
                           $"{{\"originalMessage\": \"original message here\", \"suggestedAlternative\": \"suggested rewrite\", " +
                           $"\"explanation\": \"why this alternative is better\"}}";
            
            return await RequestProcessor.ProcessModeration(this, inappropriateMessage, promptTemplate);
        }

        public async Task<string> ModerateMultiLanguageMessageAsync(string message, string detectedLanguage)
        {
            string promptTemplate = $"Moderate the following message which is in {detectedLanguage}. " +
                           $"Identify and replace any inappropriate language specific to this language and culture. " +
                           $"Return ONLY a JSON object with the structure: " +
                           $"{{\"originalMessage\": \"original message here\", \"moderatedMessage\": \"moderated message\", " +
                           $"\"language\": \"{detectedLanguage}\", \"wasModified\": true/false, " +
                           $"\"culturalContext\": \"any important cultural notes\"}}";
            
            return await RequestProcessor.ProcessModeration(this, message, promptTemplate);
        }

        public async Task<string> AnalyzeUserReputationAsync(List<string> userMessages, int priorWarnings)
        {
            string messagesFormatted = string.Join("\n", userMessages);
            
            string promptTemplate = $"Analyze the following message history from a user with {priorWarnings} prior warnings. " +
                           $"Return ONLY a JSON object with reputationScore (1-100), behaviorPatterns (array), " +
                           $"trustworthiness (low/medium/high), recommendedActions (array), and analysis (string). " +
                           $"Message history: \n{messagesFormatted}";
            
            return await GenerateJsonResponseAsync(promptTemplate);
        }
    }

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