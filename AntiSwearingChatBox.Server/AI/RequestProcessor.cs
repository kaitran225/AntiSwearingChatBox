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
                        $"You are a highly sensitive content moderator trained to detect ANY form of profanity or inappropriate language in ANY LANGUAGE with MAXIMUM SEVERITY.\n\n" +
                        $"CRITICAL TASK: This system is for a multi-language anti-swearing chatbox that MUST filter all profanity including deliberate obfuscation attempts and creative variants.\n\n" +
                        
                        $"MULTI-LANGUAGE PROFANITY DETECTION:\n" +
                        $"You MUST detect profanity across all these languages:\n" +
                        $"- English: fuck, shit, ass, bitch, etc.\n" +
                        $"- Spanish: puta, mierda, joder, coño, etc.\n" +
                        $"- French: putain, merde, salope, etc.\n" +
                        $"- German: scheiße/scheisse, arschloch, ficken, etc.\n" +
                        $"- Italian: cazzo, fanculo, stronzo, etc.\n" +
                        $"- Vietnamese: đụ/du, địt/dit, lồn/lon, dcm, đmm, etc.\n" +
                        $"- Portuguese: caralho, buceta, porra, etc.\n" +
                        $"- Russian: (transliterated) blyat, pizda, khuy, etc.\n" +
                        $"- ANY OTHER LANGUAGE not listed here\n\n" +
                        
                        $"IMPORTANT: Profanity detection is CRITICAL. The following words and their variations are ALWAYS considered profanity:\n" +
                        $"- fuck, f*ck, fuk, fvck, phuck, fcuk, f0ck, fu(k, and ANY similar variations\n" +
                        $"- shit, sh*t, sh!t, sht, and ANY similar variations\n" +
                        $"- ass, a$$, a**, @ss, and ANY similar variations\n" +
                        $"- bitch, b*tch, b!tch, and ANY similar variations\n" +
                        $"- dick, d*ck, d!ck, and ANY similar variations\n" +
                        $"- pussy, pus$y, pus*, and ANY similar variations\n" +
                        $"- cunt, c*nt, kunt, and ANY similar variations\n\n" +
                        
                        $"For EACH word, check if ANY part of the message could be:\n" +
                        $"1. THE EXACT WORD in any form (plural, possessive, etc.)\n" +
                        $"2. DELIBERATE MISSPELLINGS designed to evade filters\n" +
                        $"3. CHARACTER SUBSTITUTIONS (like @ for a, $ for s, etc.)\n" +
                        $"4. PHONETIC VARIANTS (like 'fck', 'phuk', 'shi+', etc.)\n" +
                        $"5. SPLIT WORDS with spaces or punctuation (f u c k, s.h.i.t)\n" +
                        $"6. L33T SPEAK variants (f4ck, sh1t, @ss, etc.)\n" +
                        $"7. MULTI-LANGUAGE variants or transliterations\n" +
                        $"8. PARTIAL MATCHES that clearly suggest profanity\n" +
                        
                        $"Carefully analyze this message for ALL variations of profanity including:\n" +
                        $"- Common misspellings (e.g., 'fuk', 'fvck', 'phuck', 'fcuk', 'f0ck')\n" +
                        $"- Letter repetition (e.g., 'fuckk', 'fuuuck')\n" +
                        $"- Letter substitutions (e.g., 'f*ck', 'fu¢k', 'f@ck')\n" +
                        $"- Character omissions (e.g., 'fk', 'fck')\n" +
                        $"- Word fragments that suggest profanity\n\n" +
                        
                        $"WORD-BY-WORD VARIANT CHECK: For each basic profanity word, check if EVERY possible variant of it appears in the message.\n\n" +
                        
                        $"CRITICAL INSTRUCTION: In your analysis, you MUST identify the exact profanity words or phrases found in the message.\n" +
                        $"Even if you're not 100% certain, flag potential profanity. ALWAYS err on the side of caution - false positives are better than false negatives in this system.\n\n" +
                        
                        $"Message to analyze: \"{originalPrompt}\"\n\n" +
                        
                        $"QUADRUPLE-CHECK: Before responding, ask yourself if:\n" +
                        $"1. Does this message contain ANY obvious profanity or variants in ANY language?\n" +
                        $"2. Could ANY word be interpreted as a deliberately obfuscated profanity?\n" +
                        $"3. Does it contain ANY profanity terms from non-English languages?\n" +
                        $"4. When normalized (removing spaces, special chars), does ANY part match profanity?\n" +
                        $"5. Would a human moderator likely flag this message?\n" +
                        $"If ANY answer is yes, you MUST set containsProfanity to true.\n\n" +
                        
                        $"Respond with JSON containing:\n" +
                        $"- 'containsProfanity': boolean (true if ANY variation of profanity is detected)\n" +
                        $"- 'inappropriateTerms': array of strings (specific terms detected)\n" +
                        $"- 'language': string (likely language of any detected profanity)\n" +
                        $"- 'explanation': string (why the content was or wasn't flagged)\n" +
                        $"- 'originalMessage': the original message";
                    break;
                    
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
            
            // Extended list to check against direct word lists
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
                "pussy", "puss", "pu$$y", "p*ssy", "pussi", "p u s s y", "p.ssy", "p_ssy",
                
                // Multi-language profanity (Spanish)
                "puta", "mierda", "joder", "coño", "chinga", "pendejo", "cabron", "cabrón", 
                
                // Multi-language profanity (French)
                "putain", "merde", "salope", "connard", "foutre", "salaud", "enculé", "connasse",
                
                // Multi-language profanity (German)
                "scheiße", "scheisse", "arschloch", "ficken", "fick", "fotze", "schwanz", "hure",
                
                // Multi-language profanity (Italian)
                "cazzo", "fanculo", "stronzo", "merda", "puttana", "vaffanculo",
                
                // Multi-language profanity (Vietnamese)
                "đụ", "du", "địt", "dit", "lồn", "lon", "cặc", "cac", "dcm", "dmm", "du ma",
                
                // Multi-language profanity (Portuguese)
                "caralho", "porra", "foda", "buceta", "cu",
                
                // Multi-language profanity (Russian - transliterated)
                "blyat", "blyad", "pizda", "khuy", "suka", "yebat", "yebal", "mudak"
            };
            
            // Check for direct substring matches (relaxed matching)
            foreach (var word in directProfanityWords)
            {
                if (normalizedMessage.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Direct profanity word detected with contains: {word}");
                    System.Console.WriteLine($"Direct profanity word detected with contains: {word}");
                    return true;
                }
            }
            
            // Check for pattern-based evasion techniques
            var settings = ModelSettings.Instance;
            var patterns = settings.Moderation.EvasionPatterns;
            
            foreach (var pattern in patterns)
            {
                if (normalizedMessage.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Evasion pattern detected: {pattern}");
                    System.Console.WriteLine($"Evasion pattern detected: {pattern}");
                    return true;
                }
            }
            
            // Additional check for combined/spaced words (e.g., "b i t c h")
            string strippedMessage = new string(normalizedMessage.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c)).ToArray());
            string[] symbolProfanityWords = new[] { 
                "fck", "fk", "sht", "bch", "dck", "fvk", "fvck", "cnt", "fuc", "sh1t", "fug",
                "fcuk", "phck", "psy", "btch", "dik", "kunt", "asshl", "fkyou", "mthrfkr",
                // Adding specific patterns from screenshot
                "fack", "fackyou", "fakyou", "biatch", "biatches", "biatc", "fak"
            };
            
            foreach (var word in symbolProfanityWords)
            {
                if (strippedMessage.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Stripped profanity word detected: {word}");
                    System.Console.WriteLine($"Stripped profanity word detected: {word}");
                    return true;
                }
            }
            
            // Use the advanced normalization function for deep detection
            string deepNormalizedText = AntiSwearingChatBox.AI.Services.ProfanityFilterService.NormalizeTextForProfanityDetection(message);
            string[] coreWords = new[] {
                "fuck", "shit", "ass", "bitch", "dick", "cunt", "pussy", "asshole", "bastard", "piss", 
                "whore", "slut", "bullshit", "fag", "faggot", "damn", "wank", "cock", "twat", "prick"
            };
            
            foreach (var word in coreWords)
            {
                if (deepNormalizedText.Contains(word))
                {
                    System.Diagnostics.Debug.WriteLine($"Deep normalized profanity detected: {word}");
                    System.Console.WriteLine($"Deep normalized profanity detected: {word}");
                    return true;
                }
            }
            
            // Advanced check for split word detection
            string[] wordFragments = new[] {
                "fu", "fuc", "fuk", "fck", "sh", "shi", "sht", "as", "ass", "btc", "btch", "dik", "dic", 
                "cun", "cnt", "pus", "pss", "fak", "fac", "fack", "biat", "bia", "biatch", "biatc"
            };
            
            // If multiple suspicious fragments are found, consider it a detection
            int fragmentCount = 0;
            foreach (var fragment in wordFragments)
            {
                if (deepNormalizedText.Contains(fragment))
                {
                    fragmentCount++;
                    if (fragmentCount >= 2)
                    {
                        System.Diagnostics.Debug.WriteLine($"Multiple suspicious fragments detected");
                        System.Console.WriteLine($"Multiple suspicious fragments detected");
                        return true;
                    }
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