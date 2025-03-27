using System;
using System.Text.Json;
using AntiSwearingChatBox.Core.Moderation;

namespace AntiSwearingChatBox.AI
{
    /// <summary>
    /// Handles preprocessing of requests and postprocessing of responses
    /// to ensure consistent quality and accuracy in AI moderation
    /// </summary>
    public class RequestProcessor
    {
        /// <summary>
        /// Adds constraints and instructions to ensure quality and accuracy
        /// </summary>
        public static string EnhancePrompt(string originalPrompt, string originalMessage)
        {
            var settings = ModelSettings.Instance;
            string instructions = settings.Moderation.GetEffectivePromptPrefix();
            instructions += $"\n7. The original message is: \"{EscapeJsonString(originalMessage)}\"\n\n";
            var responseOptions = settings.Moderation.ResponseOptions;
            if (responseOptions.IncludeExplanations)
            {
                instructions += "8. Include explanations for why content was flagged.\n";
            }
            
            if (responseOptions.ShowConfidenceScores)
            {
                instructions += "9. Include confidence scores (0.0-1.0) for each detection.\n";
            }
            
            if (responseOptions.AlwaysShowCulturalContext)
            {
                instructions += "10. Always include cultural context when moderating non-English content.\n";
            }
            
            var filteringRules = settings.Moderation.FilteringRules;
            foreach (var rule in filteringRules)
            {
                if (rule.Enabled && rule.RuleType == "ProfanityFilter")
                {
                    if (rule.AllowedExceptions.Count > 0)
                    {
                        instructions += $"11. The following terms are allowed exceptions and should not be flagged: {string.Join(", ", rule.AllowedExceptions)}.\n";
                    }
                    
                    if (rule.AlwaysFilterTerms.Count > 0)
                    {
                        instructions += $"12. The following terms should always be filtered: {string.Join(", ", rule.AlwaysFilterTerms)}.\n";
                    }
                }
            }
            string enhancedPrompt = instructions + originalPrompt;

            return enhancedPrompt;
        }

        /// <summary>
        /// Process a moderation response to ensure it's valid and accurate
        /// </summary>
        public static async Task<string> ProcessModeration(GeminiService service, string message, string promptTemplate)
        {
            string enhancedPrompt = EnhancePrompt(promptTemplate, message);
            
            string response = await service.GenerateJsonResponseAsync(enhancedPrompt);
            
            return ValidateAndFixResponse(response, message);
        }

        /// <summary>
        /// Validate response JSON and fix common issues
        /// </summary>
        private static string ValidateAndFixResponse(string response, string originalMessage)
        {
            try
            {
                using var doc = JsonDocument.Parse(response);
                
                var settings = ModelSettings.Instance;
                if (settings.Moderation.ResponseOptions.PreserveOriginalText)
                {
                    if (doc.RootElement.TryGetProperty("originalMessage", out var originalInResponse) ||
                        doc.RootElement.TryGetProperty("original", out originalInResponse))
                    {
                        string originalInResponseStr = originalInResponse.GetString() ?? "";
                        
                        if (!IsCloseMatch(originalInResponseStr, originalMessage) && 
                            originalInResponseStr.Length > 0 && originalMessage.Length > 0)
                        {
                            return UpdateOriginalMessageInJson(response, originalMessage);
                        }
                    }
                }
                
                return response; // Valid and no issues detected
            }
            catch
            {
                return CreateBasicResponse(originalMessage);
            }
        }

        /// <summary>
        /// Check if two strings are relatively close matches
        /// </summary>
        private static bool IsCloseMatch(string str1, string str2)
        {
            if (str1.Length < 5 || str2.Length < 5)
                return str1 == str2;
                
            int minLength = Math.Min(str1.Length, str2.Length);
            int maxLength = Math.Max(str1.Length, str2.Length);
            
            var settings = ModelSettings.Instance;
            double matchThreshold = 1.3; // Default
            
            if (settings.Moderation.Sensitivity == "High")
            {
                matchThreshold = 1.1; // More strict for high sensitivity
            }
            else if (settings.Moderation.Sensitivity == "Low")
            {
                matchThreshold = 1.5; // More lenient for low sensitivity
            }
            
            if (minLength * matchThreshold < maxLength)
                return false;
            return str1.Contains(str2) || str2.Contains(str1);
        }

        /// <summary>
        /// Update the original message in a JSON response
        /// </summary>
        private static string UpdateOriginalMessageInJson(string jsonResponse, string correctOriginalMessage)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                using var stream = new System.IO.MemoryStream();
                using var writer = new Utf8JsonWriter(stream);
                
                writer.WriteStartObject();
                
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Name.ToLower() == "originalmessage" || property.Name.ToLower() == "original")
                    {
                        writer.WriteString(property.Name, correctOriginalMessage);
                    }
                    else
                    {
                        property.WriteTo(writer);
                    }
                }
                
                writer.WriteEndObject();
                writer.Flush();
                
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                return CreateBasicResponse(correctOriginalMessage);
            }
        }

        /// <summary>
        /// Create a basic valid JSON response when more complex processing fails
        /// </summary>
        private static string CreateBasicResponse(string originalMessage)
        {
            var basicResponse = new
            {
                originalMessage = originalMessage,
                moderatedMessage = originalMessage,
                wasModified = false,
                error = "Failed to process response properly"
            };
            
            return JsonSerializer.Serialize(basicResponse, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Escape special characters for JSON string
        /// </summary>
        private static string EscapeJsonString(string str)
        {
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
} 