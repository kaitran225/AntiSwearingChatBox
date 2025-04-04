using System;
using System.Text.Json;
using AntiSwearingChatBox.AI.Moderation;

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
        public static string EnhancePrompt(string originalPrompt, string messageType)
        {
            if (string.IsNullOrEmpty(originalPrompt))
                return originalPrompt;
                
            string enhancedPrompt = originalPrompt;
            
            // Add special instructions based on message type
            switch (messageType.ToLower())
            {
                case "profanity":
                    enhancedPrompt = 
                        $"You are a highly sensitive content moderator trained to detect ANY form of profanity or inappropriate language.\n\n" +
                        $"IMPORTANT: Profanity detection is CRITICAL. The following words and their variations are ALWAYS considered profanity:\n" +
                        $"- fuck, f*ck, fuk, fvck, phuck, fcuk, f0ck, fu(k, and ANY similar variations\n" +
                        $"- shit, sh*t, sh!t, sht, and ANY similar variations\n" +
                        $"- ass, a$$, a**, @ss, and ANY similar variations\n" +
                        $"- bitch, b*tch, b!tch, and ANY similar variations\n" +
                        $"- dick, d*ck, d!ck, and ANY similar variations\n\n" +
                        
                        $"Carefully analyze this message for ALL variations of profanity including:\n" +
                        $"- Common misspellings (e.g., 'fuk', 'fvck', 'phuck', 'fcuk', 'f0ck')\n" +
                        $"- Letter repetition (e.g., 'fuckk', 'fuuuck')\n" +
                        $"- Letter substitutions (e.g., 'f*ck', 'fu¢k', 'f@ck')\n" +
                        $"- Character omissions (e.g., 'fk', 'fck')\n" +
                        $"- Word fragments that suggest profanity\n\n" +
                        
                        $"CRITICAL INSTRUCTION: In your analysis, you MUST identify the exact profanity words or phrases found in the message.\n" +
                        $"Even if you're not 100% certain, flag potential profanity. Err on the side of caution.\n\n" +
                        
                        $"Message to analyze: \"{originalPrompt}\"\n\n" +
                        
                        $"FINAL CHECK: Before responding, ask yourself - does this message contain obvious profanity like 'fuck', 'shit', etc.? If so, you MUST set containsProfanity to true.\n\n" +
                        
                        $"Respond with JSON containing:\n" +
                        $"- 'containsProfanity': boolean (true if ANY variation of profanity is detected)\n" +
                        $"- 'inappropriateTerms': array of strings (specific terms detected)\n" +
                        $"- 'explanation': string (why the content was or wasn't flagged)\n" +
                        $"- 'originalMessage': the original message";
                    break;
                    
                // ... keep other existing cases ...
            }
            
            Console.WriteLine($"Enhanced prompt for message: \"{originalPrompt}\"");
            return enhancedPrompt;
        }

        /// <summary>
        /// Process a moderation response to ensure it's valid and accurate
        /// </summary>
        public static async Task<string> ProcessModeration(GeminiService service, string message, string promptTemplate)
        {
            string enhancedPrompt = EnhancePrompt(promptTemplate, message);
            
            try
            {
                string response = await service.GenerateJsonResponseAsync(enhancedPrompt);
                System.Diagnostics.Debug.WriteLine($"Raw AI response: {response}");
                System.Console.WriteLine($"Raw AI response: {response}");
                
                return ValidateAndFixResponse(response, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ProcessModeration: {ex.Message}");
                System.Console.WriteLine($"Error in ProcessModeration: {ex.Message}");
                
                // Create a fallback response that indicates profanity was detected
                // This is our conservative approach when AI fails
                return CreateFallbackResponse(message);
            }
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
                
                // Check if we should override AI's decision for known evasion patterns
                bool containsKnownEvasion = ContainsKnownEvasionPatterns(originalMessage);
                if (containsKnownEvasion)
                {
                    // For detection endpoint
                    if (doc.RootElement.TryGetProperty("containsProfanity", out var containsProfanity))
                    {
                        bool currentValue = containsProfanity.GetBoolean();
                        if (!currentValue)
                        {
                            System.Diagnostics.Debug.WriteLine($"Overriding AI decision due to evasion pattern in: \"{originalMessage}\"");
                            System.Console.WriteLine($"Overriding AI decision due to evasion pattern in: \"{originalMessage}\"");
                            return UpdateProfanityDetectionInJson(response, true);
                        }
                    }
                    
                    // For moderation endpoint
                    if (doc.RootElement.TryGetProperty("wasModified", out var wasModified))
                    {
                        bool currentValue = wasModified.GetBoolean();
                        if (!currentValue)
                        {
                            System.Diagnostics.Debug.WriteLine($"Overriding AI moderation due to evasion pattern in: \"{originalMessage}\"");
                            System.Console.WriteLine($"Overriding AI moderation due to evasion pattern in: \"{originalMessage}\"");
                            
                            // Create censored text
                            string censored = new string('*', originalMessage.Length);
                            return UpdateModerationInJson(response, censored, true);
                        }
                    }
                }
                
                return response; // Valid and no issues detected
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating response: {ex.Message}");
                System.Console.WriteLine($"Error validating response: {ex.Message}");
                return CreateFallbackResponse(originalMessage);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating original message: {ex.Message}");
                System.Console.WriteLine($"Error updating original message: {ex.Message}");
                return CreateFallbackResponse(correctOriginalMessage);
            }
        }

        /// <summary>
        /// Update containsProfanity in a JSON response
        /// </summary>
        private static string UpdateProfanityDetectionInJson(string jsonResponse, bool containsProfanity)
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
                    if (property.Name.ToLower() == "containsprofanity")
                    {
                        writer.WriteBoolean(property.Name, containsProfanity);
                    }
                    else if (containsProfanity && property.Name.ToLower() == "explanation" && !property.Value.GetString().Contains("override"))
                    {
                        writer.WriteString(property.Name, property.Value.GetString() + " (Override: Known evasion pattern detected)");
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
                // If we can't update the JSON, create a new response
                var response = new
                {
                    containsProfanity = containsProfanity,
                    inappropriateTerms = new[] { "detected evasion pattern" },
                    explanation = "Override: Known evasion pattern detected",
                    originalMessage = jsonResponse
                };
                
                return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Update moderation in a JSON response
        /// </summary>
        private static string UpdateModerationInJson(string jsonResponse, string moderatedText, bool wasModified)
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
                    if (property.Name.ToLower() == "moderated" || property.Name.ToLower() == "moderatedmessage")
                    {
                        writer.WriteString(property.Name, moderatedText);
                    }
                    else if (property.Name.ToLower() == "wasmodified")
                    {
                        writer.WriteBoolean(property.Name, wasModified);
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
                // If we can't update the JSON, create a new response
                var response = new
                {
                    original = jsonResponse,
                    moderated = moderatedText,
                    wasModified = wasModified,
                    overrideReason = "Known evasion pattern detected"
                };
                
                return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        /// <summary>
        /// Check if the message contains known profanity evasion patterns
        /// </summary>
        private static bool ContainsKnownEvasionPatterns(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;
                
            // First perform a very direct check for common profanity words
            // This is a fallback in case AI misses obvious profanity
            string normalizedMessage = message.ToLower().Replace(" ", "");
            string[] directProfanityWords = new[] { 
                "fuck", "fuk", "fvck", "f*ck", "f**k", "fck", "fuuck", "fuuk", "phuck", "fu(k",
                "shit", "sh*t", "sh!t", "sht", "sh1t", "shiit",
                "ass", "a$$", "a**", "@ss",
                "bitch", "b*tch", "b!tch", "btch",
                "dick", "d*ck", "d!ck"
            };
            
            foreach (var word in directProfanityWords)
            {
                if (normalizedMessage.Contains(word))
                {
                    System.Diagnostics.Debug.WriteLine($"Direct profanity word detected: {word}");
                    System.Console.WriteLine($"Direct profanity word detected: {word}");
                    return true;
                }
            }
                
            // Now check for pattern-based evasion techniques
            // Get the configured evasion patterns from settings
            var settings = ModelSettings.Instance;
            var patterns = settings.Moderation.EvasionPatterns;
            
            foreach (var pattern in patterns)
            {
                if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Evasion pattern detected: {pattern}");
                    System.Console.WriteLine($"Evasion pattern detected: {pattern}");
                    return true;
                }
            }
            
            // Check for symbol substitution patterns
            string symbolsRemoved = new string(message.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            string[] symbolProfanityWords = new[] { "fck", "fk", "sht", "st", "bch", "dck" };
            
            foreach (var word in symbolProfanityWords)
            {
                if (symbolsRemoved.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Symbol substitution detected after cleanup: {word}");
                    System.Console.WriteLine($"Symbol substitution detected after cleanup: {word}");
                    return true;
                }
            }
            
            return false;
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
        /// Create a fallback response that censors the message - used when AI fails
        /// </summary>
        private static string CreateFallbackResponse(string originalMessage)
        {
            // When AI fails, we take a conservative approach and censor the whole message
            string censored = new string('*', originalMessage.Length);
            
            var fallbackResponse = new
            {
                originalMessage = originalMessage,
                moderatedMessage = censored,
                wasModified = true,
                containsProfanity = true,
                explanation = "AI processing failed; conservative censoring applied for safety"
            };
            
            System.Diagnostics.Debug.WriteLine($"Created fallback response for: \"{originalMessage}\"");
            System.Console.WriteLine($"Created fallback response for: \"{originalMessage}\"");
            
            return JsonSerializer.Serialize(fallbackResponse, new JsonSerializerOptions { WriteIndented = true });
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