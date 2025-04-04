using System;
using System.Text.Json;
using AntiSwearingChatBox.AI.Moderation;
using AntiSwearingChatBox.Server.AI;

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
            // IMPORTANT: First check for profanity before involving the AI
            // This ensures we catch obvious profanity immediately
            if (ContainsKnownEvasionPatterns(message))
            {
                System.Diagnostics.Debug.WriteLine($"Direct profanity detection caught bad word in: \"{message}\"");
                System.Console.WriteLine($"Direct profanity detection caught bad word in: \"{message}\"");
                
                // Create a direct response with true for profanity detection
                if (promptTemplate.Contains("containsProfanity"))
                {
                    // This is a profanity detection request
                    var directResponse = new
                    {
                        containsProfanity = true,
                        inappropriateTerms = new[] { "detected by direct pattern matching" },
                        explanation = "Direct pattern matching detected inappropriate language",
                        originalMessage = message
                    };
                    return JsonSerializer.Serialize(directResponse, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            
            // Continue with AI-based detection if direct detection didn't catch anything
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
        /// Validate response JSON and fix common issues, returning detailed information about the process
        /// </summary>
        public static string ValidateAndFixResponseWithDetails(string response, string originalMessage, out object processingDetails)
        {
            var detailsList = new List<object>();
            try
            {
                detailsList.Add(new {
                    operation = "Parsing JSON response",
                    status = "Attempting"
                });
                
                using var doc = JsonDocument.Parse(response);
                
                detailsList.Add(new {
                    operation = "Parsing JSON response",
                    status = "Success"
                });
                
                var settings = ModelSettings.Instance;
                detailsList.Add(new {
                    operation = "Checking model settings",
                    modelSettings = new {
                        preserveOriginalText = settings.Moderation.ResponseOptions.PreserveOriginalText,
                        sensitivity = settings.Moderation.Sensitivity
                    }
                });
                
                // Check if original message needs to be fixed
                bool originalTextFixed = false;
                if (settings.Moderation.ResponseOptions.PreserveOriginalText)
                {
                    if (doc.RootElement.TryGetProperty("originalMessage", out var originalInResponse) ||
                        doc.RootElement.TryGetProperty("original", out originalInResponse))
                    {
                        string originalInResponseStr = originalInResponse.GetString() ?? "";
                        
                        if (!IsCloseMatch(originalInResponseStr, originalMessage) && 
                            originalInResponseStr.Length > 0 && originalMessage.Length > 0)
                        {
                            originalTextFixed = true;
                            detailsList.Add(new {
                                operation = "Original message check",
                                status = "Fixed",
                                aiVersion = originalInResponseStr,
                                correctVersion = originalMessage,
                                reason = "Original message in AI response didn't match the actual message"
                            });
                            
                            response = UpdateOriginalMessageInJson(response, originalMessage);
                        }
                        else
                        {
                            detailsList.Add(new {
                                operation = "Original message check",
                                status = "Passed",
                                message = "Original message in AI response matches the actual message"
                            });
                        }
                    }
                }
                
                // Check if we should override AI's decision for known evasion patterns
                bool containsKnownEvasion = ContainsKnownEvasionPatterns(originalMessage);
                bool decisionOverridden = false;
                
                detailsList.Add(new {
                    operation = "Evasion pattern check",
                    status = containsKnownEvasion ? "Detected" : "None found",
                    patternFound = containsKnownEvasion
                });
                
                if (containsKnownEvasion)
                {
                    // For detection endpoint
                    if (doc.RootElement.TryGetProperty("containsProfanity", out var containsProfanity))
                    {
                        bool currentValue = containsProfanity.GetBoolean();
                        if (!currentValue)
                        {
                            decisionOverridden = true;
                            detailsList.Add(new {
                                operation = "AI decision override",
                                status = "Applied",
                                reason = "AI didn't detect known evasion pattern",
                                aiDecision = currentValue,
                                overriddenTo = true
                            });
                            
                            System.Diagnostics.Debug.WriteLine($"Overriding AI decision due to evasion pattern in: \"{originalMessage}\"");
                            System.Console.WriteLine($"Overriding AI decision due to evasion pattern in: \"{originalMessage}\"");
                            response = UpdateProfanityDetectionInJson(response, true);
                        }
                        else
                        {
                            detailsList.Add(new {
                                operation = "AI decision check",
                                status = "Correct",
                                message = "AI correctly identified profanity"
                            });
                        }
                    }
                    
                    // For moderation endpoint
                    if (doc.RootElement.TryGetProperty("wasModified", out var wasModified))
                    {
                        bool currentValue = wasModified.GetBoolean();
                        if (!currentValue)
                        {
                            decisionOverridden = true;
                            detailsList.Add(new {
                                operation = "Moderation override",
                                status = "Applied",
                                reason = "AI didn't flag known evasion pattern for moderation",
                                aiDecision = currentValue,
                                overriddenTo = true
                            });
                            
                            System.Diagnostics.Debug.WriteLine($"Overriding AI moderation due to evasion pattern in: \"{originalMessage}\"");
                            System.Console.WriteLine($"Overriding AI moderation due to evasion pattern in: \"{originalMessage}\"");
                            
                            // Create censored text
                            string censored = new string('*', originalMessage.Length);
                            response = UpdateModerationInJson(response, censored, true);
                        }
                        else
                        {
                            detailsList.Add(new {
                                operation = "Moderation check",
                                status = "Correct",
                                message = "AI correctly moderated content"
                            });
                        }
                    }
                }
                
                // Summarize all processing actions
                processingDetails = new {
                    originalTextFixed,
                    containsKnownEvasion,
                    decisionOverridden,
                    processingSteps = detailsList
                };
                
                return response; // Valid and all issues handled
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating response: {ex.Message}");
                System.Console.WriteLine($"Error validating response: {ex.Message}");
                
                detailsList.Add(new {
                    operation = "Error handling",
                    status = "Failed",
                    error = ex.Message,
                    action = "Creating fallback response"
                });
                
                processingDetails = new {
                    error = ex.Message,
                    processingSteps = detailsList
                };
                
                return CreateFallbackResponse(originalMessage);
            }
        }

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
                
                bool containsKnownEvasion = ContainsKnownEvasionPatterns(originalMessage);
                if (containsKnownEvasion)
                {
                    if (doc.RootElement.TryGetProperty("containsProfanity", out var containsProfanity))
                    {
                        bool currentValue = containsProfanity.GetBoolean();
                        if (!currentValue)
                        {
                            return UpdateProfanityDetectionInJson(response, true);
                        }
                    }
                    
                    if (doc.RootElement.TryGetProperty("wasModified", out var wasModified))
                    {
                        bool currentValue = wasModified.GetBoolean();
                        if (!currentValue)
                        {
                            string censored = new string('*', originalMessage.Length);
                            return UpdateModerationInJson(response, censored, true);
                        }
                    }
                }
                
                return response; // Valid and no issues detected
            }
            catch (Exception)
            {
                return CreateFallbackResponse(originalMessage);
            }
        }

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
                    else if (containsProfanity && property.Name.ToLower() == "explanation" && !property.Value.GetString()!.Contains("override"))
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
                
            // First normalize the message for better detection
            string normalizedMessage = message.ToLower();
            
            // Extended list of profanity words with more variations
            string[] directProfanityWords = new[] { 
                // F-word variations
                "fuck", "fuk", "fvck", "f*ck", "f**k", "fck", "fuuck", "fuuk", "phuck", "fu(k", "fug", "fuc", 
                "f u c k", "f-uck", "f.uck", "f_uck", "fuhk", "fuhck", "fock", "f0ck", "effing", "fxck", "fuq",
                
                // S-word variations
                "shit", "sh*t", "sh!t", "sht", "sh1t", "shiit", "shyt", "sh.t", "sh_t", "s h i t", "sh1t", "schit",
                
                // A-word variations
                "ass", "a$$", "a**", "@ss", "azz", "a$", "a**", "as$", "@ss", "@$$", "a s s", "a.ss", "a_ss",
                
                // B-word variations
                "bitch", "b*tch", "b!tch", "btch", "b1tch", "biatch", "bytch", "b i t c h", "b.tch", "b_tch", "bicth",
                
                // D-word variations
                "dick", "d*ck", "d!ck", "dck", "d1ck", "diick", "dikk", "dik", "d i c k", "d.ck", "d_ck",
                
                // Additional profanities
                "cunt", "c*nt", "kunt", "cnut", "c u n t", "c-nt", "c.nt", "c_nt",
                "pussy", "puss", "pu$$y", "p*ssy", "pussi", "p u s s y", "p.ssy", "p_ssy"
            };
            
            // Use proper word boundary checking with relaxed rules to catch more variations
            foreach (var word in directProfanityWords)
            {
                // More relaxed pattern matching to catch even partial matches within words
                if (normalizedMessage.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Direct profanity word detected with contains: {word}");
                    System.Console.WriteLine($"Direct profanity word detected with contains: {word}");
                    return true;
                }
            }
                
            // Check for obscured profanity with letter substitutions (l33t speak)
            Dictionary<char, string[]> substitutions = new Dictionary<char, string[]>
            {
                {'a', new[] {"@", "4", "α", "λ", "д"}},
                {'b', new[] {"8", "6", "ß", "б"}},
                {'c', new[] {"(", "{", "[", "<", "к", "с"}},
                {'d', new[] {"|)", "о", "д"}},
                {'e', new[] {"3", "€", "є", "е", "э"}},
                {'f', new[] {"ph", "ƒ", "ф"}},
                {'g', new[] {"6", "9", "г"}},
                {'h', new[] {"#", "|-|", "н"}},
                {'i', new[] {"1", "!", "|", "и", "і"}},
                {'k', new[] {"|<", "к"}},
                {'l', new[] {"1", "|", "л"}},
                {'m', new[] {"|v|", "м"}},
                {'n', new[] {"|\\|", "и", "н"}},
                {'o', new[] {"0", "()", "о"}},
                {'p', new[] {"|o", "п"}},
                {'r', new[] {"|2", "я", "р"}},
                {'s', new[] {"5", "$", "с"}},
                {'t', new[] {"7", "+", "т"}},
                {'u', new[] {"|_|", "у"}},
                {'v', new[] {"\\/", "в"}},
                {'w', new[] {"\\/\\/", "vv", "ш", "щ"}},
                {'x', new[] {"><", "х"}},
                {'y', new[] {"j", "у", "ү"}},
                {'z', new[] {"2", "з"}}
            };
            
            // Check for common patterns with character replacements
            // Harder to evade by removing all special characters
            string[] corePatterns = new[] { "fk", "fck", "fvk", "fuk", "phk", "sht", "ashole", "btch", "dck", "fvck" };
            foreach (var pattern in corePatterns)
            {
                // Create variants with character substitutions
                List<string> variants = GenerateVariants(pattern, substitutions);
                
                foreach (var variant in variants)
                {
                    if (normalizedMessage.Contains(variant, StringComparison.OrdinalIgnoreCase))
                    {
                        System.Diagnostics.Debug.WriteLine($"L33t speak variant detected: {variant} from {pattern}");
                        System.Console.WriteLine($"L33t speak variant detected: {variant} from {pattern}");
                        return true;
                    }
                }
            }
            
            // Additional check for combined/spaced words (e.g., "b i t c h")
            string strippedMessage = new string(normalizedMessage.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c)).ToArray());
            foreach (var word in directProfanityWords)
            {
                // Remove spaces and punctuation from the profanity word for matching
                string strippedWord = new string(word.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c)).ToArray());
                if (strippedMessage.Contains(strippedWord, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Spaced profanity word detected: {word}");
                    System.Console.WriteLine($"Spaced profanity word detected: {word}");
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
            string[] symbolProfanityWords = new[] { 
                "fck", "fk", "sht", "bch", "dck", "fvk", "fvck", "cnt", "fuc", "sh1t", "fug",
                "fcuk", "phck", "psy"
            };
            
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
        /// Generate variations of profanity words with character substitutions
        /// </summary>
        private static List<string> GenerateVariants(string pattern, Dictionary<char, string[]> substitutions)
        {
            List<string> variants = new List<string>();
            variants.Add(pattern); // Add original pattern
            
            // Generate variations by replacing one character at a time
            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];
                if (substitutions.ContainsKey(c))
                {
                    foreach (string replacement in substitutions[c])
                    {
                        string variant = pattern.Substring(0, i) + replacement + pattern.Substring(i + 1);
                        variants.Add(variant);
                    }
                }
            }
            
            return variants;
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

        /// <summary>
        /// Public method for other classes to check for profanity
        /// </summary>
        public static bool ContainsDirectProfanity(string message)
        {
            return ContainsKnownEvasionPatterns(message);
        }
    }
} 