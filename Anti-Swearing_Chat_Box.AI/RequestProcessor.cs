using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anti_Swearing_Chat_Box.Core.Moderation;

namespace Anti_Swearing_Chat_Box.AI
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

            // Get the AI instructions from the settings
            string instructions = settings.Moderation.GetEffectivePromptPrefix();
            
            // Add the original message as a specific instruction
            instructions += $"\n7. The original message is: \"{EscapeJsonString(originalMessage)}\"\n\n";
            
            // Add custom response format options based on settings
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
            
            // Add filtering rules based on settings
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
            
            // Add the original prompt
            string enhancedPrompt = instructions + originalPrompt;

            return enhancedPrompt;
        }

        /// <summary>
        /// Process a moderation response to ensure it's valid and accurate
        /// </summary>
        public static async Task<string> ProcessModeration(GeminiService service, string message, string promptTemplate)
        {
            // Create an enhanced prompt with constraints
            string enhancedPrompt = EnhancePrompt(promptTemplate, message);
            
            // Get the response from the AI
            string response = await service.GenerateJsonResponseAsync(enhancedPrompt);
            
            // Validate the response and fix common issues
            return ValidateAndFixResponse(response, message);
        }

        /// <summary>
        /// Validate response JSON and fix common issues
        /// </summary>
        private static string ValidateAndFixResponse(string response, string originalMessage)
        {
            try
            {
                // First check if it's valid JSON
                using var doc = JsonDocument.Parse(response);
                
                // If we got here, it's valid JSON, but let's check for common issues
                
                // Only perform this check if the preserve original text setting is enabled
                var settings = ModelSettings.Instance;
                if (settings.Moderation.ResponseOptions.PreserveOriginalText)
                {
                    // 1. Check if the original message is accurately represented
                    if (doc.RootElement.TryGetProperty("originalMessage", out var originalInResponse) ||
                        doc.RootElement.TryGetProperty("original", out originalInResponse))
                    {
                        string originalInResponseStr = originalInResponse.GetString() ?? "";
                        
                        // If the original message is significantly different, create a corrected response
                        if (!IsCloseMatch(originalInResponseStr, originalMessage) && 
                            originalInResponseStr.Length > 0 && originalMessage.Length > 0)
                        {
                            // Try to fix by updating the original message while keeping the rest of the response
                            return UpdateOriginalMessageInJson(response, originalMessage);
                        }
                    }
                }
                
                return response; // Valid and no issues detected
            }
            catch
            {
                // If parsing fails, create a basic valid JSON response
                return CreateBasicResponse(originalMessage);
            }
        }

        /// <summary>
        /// Check if two strings are relatively close matches
        /// </summary>
        private static bool IsCloseMatch(string str1, string str2)
        {
            // For very short strings, require exact match
            if (str1.Length < 5 || str2.Length < 5)
                return str1 == str2;
                
            // For longer strings, allow some variance
            // Using Levenshtein distance would be better, but this is a simple approach
            int minLength = Math.Min(str1.Length, str2.Length);
            int maxLength = Math.Max(str1.Length, str2.Length);
            
            // Get sensitivity level from settings
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
            
            // If length difference is more than the threshold, they're not close
            if (minLength * matchThreshold < maxLength)
                return false;
                
            // Simple substring check
            return str1.Contains(str2) || str2.Contains(str1);
        }

        /// <summary>
        /// Update the original message in a JSON response
        /// </summary>
        private static string UpdateOriginalMessageInJson(string jsonResponse, string correctOriginalMessage)
        {
            try
            {
                // Parse JSON into a mutable document
                using var doc = JsonDocument.Parse(jsonResponse);
                
                // Create a modified version
                var options = new JsonSerializerOptions { WriteIndented = true };
                using var stream = new System.IO.MemoryStream();
                using var writer = new Utf8JsonWriter(stream);
                
                writer.WriteStartObject();
                
                // Write all properties, but replace originalMessage/original with the correct one
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
                
                // Convert back to string
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                // If something goes wrong, fall back to creating a basic response
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