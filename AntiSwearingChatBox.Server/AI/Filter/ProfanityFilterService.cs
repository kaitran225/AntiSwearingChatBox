using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using AntiSwearingChatBox.AI.Filter;

namespace AntiSwearingChatBox.AI.Services;

/// <summary>
/// AI-based profanity filter service that uses Gemini for content moderation
/// </summary>
public class ProfanityFilterService : IProfanityFilter
{
    private readonly GeminiService? _geminiService;
    private const int MAX_RETRY_ATTEMPTS = 3;
    private readonly List<string> _profanityWords = new List<string> { "fuck", "shit", "ass", "bitch", "dick", "cunt" };

    public ProfanityFilterService(GeminiService? geminiService = null)
    {
        _geminiService = geminiService;
        
        // Log startup status
        if (_geminiService != null)
        {
            System.Diagnostics.Debug.WriteLine("AI-ONLY MODE: ProfanityFilterService initialized with GeminiService");
            System.Console.WriteLine("AI-ONLY MODE: ProfanityFilterService initialized with GeminiService");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ERROR: AI-ONLY MODE requires GeminiService but it was not provided");
            System.Console.WriteLine("ERROR: AI-ONLY MODE requires GeminiService but it was not provided");
            throw new System.InvalidOperationException("AI-ONLY MODE requires GeminiService but it was not provided");
        }
    }

    /// <inheritdoc />
    public async Task<bool> ContainsProfanityAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        
        // Use the existing DetectProfanityWithAIAsync method
        return await DetectProfanityWithAIAsync(text);
    }

    /// <inheritdoc />
    public async Task<string> FilterTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
            
        // Ensure GeminiService exists
        if (_geminiService == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: GeminiService is null, cannot perform AI text moderation");
            System.Console.WriteLine("ERROR: GeminiService is null, cannot perform AI text moderation");
            throw new System.InvalidOperationException("GeminiService is required for AI-ONLY mode");
        }
            
        // AI-based moderation with multiple retries
        for (int attempt = 0; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AI Attempt {attempt+1}: Moderating text: \"{text}\"");
                System.Console.WriteLine($"AI Attempt {attempt+1}: Moderating text: \"{text}\"");
                
                var result = await _geminiService.ModerateChatMessageAsync(text);
                System.Diagnostics.Debug.WriteLine($"AI moderation raw result: {result}");
                
                try
                {
                    var jsonResult = JsonDocument.Parse(result);
                    
                    if (jsonResult.RootElement.TryGetProperty("moderated", out var moderated) || 
                        jsonResult.RootElement.TryGetProperty("moderatedMessage", out moderated))
                    {
                        string moderatedText = moderated.GetString() ?? text;
                        System.Diagnostics.Debug.WriteLine($"AI moderated text: \"{moderatedText}\"");
                        System.Console.WriteLine($"AI moderated text: \"{moderatedText}\"");
                        return moderatedText;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("AI response missing moderated/moderatedMessage property");
                        System.Console.WriteLine("AI response missing moderated/moderatedMessage property");
                    }
                }
                catch (System.Text.Json.JsonException jex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing AI JSON response: {jex.Message}");
                    System.Console.WriteLine($"Error parsing AI JSON response: {jex.Message}");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AI moderation attempt {attempt+1} failed: {ex.Message}");
                System.Console.WriteLine($"AI moderation attempt {attempt+1} failed: {ex.Message}");
                
                if (attempt < MAX_RETRY_ATTEMPTS)
                {
                    // Wait before retry with increasing delay
                    await Task.Delay(500 * (attempt + 1));
                }
            }
        }
        
        // If we reach here after all retries, we'll censor the whole text as a conservative approach
        string censored = new string('*', text.Length);
        System.Diagnostics.Debug.WriteLine($"All AI attempts failed to moderate, conservatively censoring text: \"{text}\" → \"{censored}\"");
        System.Console.WriteLine($"All AI attempts failed to moderate, conservatively censoring text: \"{text}\" → \"{censored}\"");
        return censored;
    }
    
    /// <inheritdoc />
    public async Task<string> FilterProfanityAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        try
        {
            Console.WriteLine($"Checking message for profanity: \"{text}\"");
            
            // If no AI service is available, use regex-based filtering
            if (_geminiService == null)
            {
                Console.WriteLine("No AI service available, using regex-based filtering");
                return FilterProfanityWithRegex(text);
            }
            
            // Check if message contains profanity using AI
            bool containsProfanity = await DetectProfanityWithAIAsync(text);
            
            // If AI says it contains profanity, attempt to moderate with AI
            if (containsProfanity)
            {
                try
                {
                    // Try to use AI to moderate the content
                    var moderatedText = await _geminiService.ModerateChatMessageAsync(text);
                    
                    // Extract just the moderated text from the JSON response
                    var moderationResult = JsonSerializer.Deserialize<ModerationResult>(moderatedText);
                    
                    if (moderationResult != null && !string.IsNullOrEmpty(moderationResult.ModeratedMessage))
                    {
                        Console.WriteLine($"AI moderated message from \"{text}\" to \"{moderationResult.ModeratedMessage}\"");
                        return moderationResult.ModeratedMessage;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in AI moderation: {ex.Message}, falling back to regex filtering");
                }
                
                // If AI moderation fails, fall back to regex
                return FilterProfanityWithRegex(text);
            }
            
            // If no profanity detected, return the original text
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in profanity filtering: {ex.Message}, returning original text");
            return text;
        }
    }

    private string FilterProfanityWithRegex(string text)
    {
        string filteredText = text;
        
        // Filter using regex patterns
        foreach (var word in _profanityWords)
        {
            // Match whole words only with word boundaries
            filteredText = Regex.Replace(
                filteredText, 
                $"\\b{Regex.Escape(word)}\\b", 
                match => new string('*', match.Length),
                RegexOptions.IgnoreCase
            );
        }
        
        // Handle l33t speak and other variations
        var patterns = new[]
        {
            @"f[^\w]*u[^\w]*c[^\w]*k",
            @"s[^\w]*h[^\w]*i[^\w]*t",
            @"a[^\w]*s[^\w]*s",
            @"b[^\w]*i[^\w]*t[^\w]*c[^\w]*h",
            @"d[^\w]*i[^\w]*c[^\w]*k",
            @"c[^\w]*u[^\w]*n[^\w]*t"
        };
        
        foreach (var pattern in patterns)
        {
            filteredText = Regex.Replace(
                filteredText, 
                pattern, 
                match => new string('*', match.Length),
                RegexOptions.IgnoreCase
            );
        }
        
        return filteredText;
    }

    private async Task<bool> DetectProfanityWithAIAsync(string message)
    {
        // For moderately short messages, still use AI detection
        // We want to make sure we catch short profanity like "fuk"
        if (message.Length < 5)
        {
            Console.WriteLine($"Message too short to use AI: \"{message}\"");
            return ContainsObviousProfanity(message);
        }
        
        int maxAttempts = 2; // Reduce from 4 to 2 attempts to conserve quota
        int currentAttempt = 0;
        bool useAI = true;
        
        while (currentAttempt < maxAttempts && useAI)
        {
            currentAttempt++;
            try
            {
                Console.WriteLine($"AI Attempt {currentAttempt}: Detecting profanity in: \"{message}\"");
                
                // Call the AI service to detect profanity
                var jsonResponse = await _geminiService?.DetectProfanityAsync(message);
                Console.WriteLine($"Raw AI response: {jsonResponse}");
                
                // Try to extract the profanity detection result
                bool? containsProfanity = ExtractContainsProfanityFromResponse(jsonResponse);
                
                if (containsProfanity.HasValue)
                {
                    // If AI explicitly says it doesn't contain profanity, double-check common evasions
                    if (!containsProfanity.Value)
                    {
                        // Check common evasion patterns as a double-check
                        bool hasObviousProfanity = ContainsObviousProfanity(message);
                        if (hasObviousProfanity)
                        {
                            Console.WriteLine($"AI missed obvious profanity in: \"{message}\", overriding to true");
                            return true;
                        }
                    }
                    return containsProfanity.Value;
                }
                
                Console.WriteLine("AI response missing containsProfanity property");
                
                // If we get a rate limit error (429), switch to regex
                if (jsonResponse.Contains("429") || jsonResponse.Contains("RESOURCE_EXHAUSTED"))
                {
                    Console.WriteLine("Rate limit reached for AI API, falling back to regex");
                    useAI = false;
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AI profanity detection: {ex.Message}");
                if (ex.Message.Contains("429") || ex.Message.Contains("rate limit") || ex.Message.Contains("quota"))
                {
                    Console.WriteLine("Rate limit reached for AI API, falling back to regex");
                    useAI = false;
                    break;
                }
            }
        }
        
        // If AI failed or rate limited, use regex fallback
        if (currentAttempt >= maxAttempts || !useAI)
        {
            Console.WriteLine($"Falling back to regex-based profanity detection for: \"{message}\"");
            return ContainsObviousProfanity(message);
        }
        
        // Changed from true to false as a more lenient fallback
        // BUT if the message looks suspicious, still flag it
        if (ContainsSuspiciousPatterns(message))
        {
            Console.WriteLine($"Message contains suspicious patterns, conservatively returning true: \"{message}\"");
            return true;
        }
        
        Console.WriteLine($"All AI attempts failed to detect profanity, defaulting to false for: \"{message}\"");
        return false;
    }

    private bool ContainsSuspiciousPatterns(string message)
    {
        string lowerMessage = message.ToLower();
        
        // Check for partial matches that might indicate evasion attempts
        var suspiciousPatterns = new[]
        {
            @"f\W*[vu@o0]",       // Starts with f + vowel (likely profanity)
            @"[vu@o0]\W*c?k",     // Ends with c/k pattern (likely profanity)
            @"sh\W*[i1!]",        // Starts with shi pattern
            @"a\W*s\W*s",         // a*s*s pattern
            @"b\W*[i1!]\W*t\W*c", // b*i*t*c pattern
        };
        
        foreach (var pattern in suspiciousPatterns)
        {
            if (Regex.IsMatch(lowerMessage, pattern))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool? ExtractContainsProfanityFromResponse(string jsonResponse)
    {
        if (string.IsNullOrEmpty(jsonResponse))
            return null;
        
        try
        {
            // Handle different response formats
            
            // Format 1: Direct JSON response
            if (jsonResponse.TrimStart().StartsWith("{"))
            {
                var directResponse = JsonSerializer.Deserialize<ProfanityDetectionResult>(jsonResponse);
                if (directResponse?.ContainsProfanity != null)
                    return directResponse.ContainsProfanity;
            }
            
            // Format 2: JSON embedded in a text property
            if (jsonResponse.Contains("\"text\""))
            {
                var wrapper = JsonSerializer.Deserialize<TextWrapper>(jsonResponse);
                if (!string.IsNullOrEmpty(wrapper?.Text))
                {
                    // Extract JSON from markdown code blocks if present
                    string text = wrapper.Text;
                    if (text.Contains("```json") && text.Contains("```"))
                    {
                        int start = text.IndexOf("```json") + 7;
                        int end = text.LastIndexOf("```");
                        if (start > 7 && end > start)
                        {
                            text = text.Substring(start, end - start).Trim();
                        }
                    }
                    
                    // Try to parse the text as JSON
                    var embeddedResponse = JsonSerializer.Deserialize<ProfanityDetectionResult>(text);
                    if (embeddedResponse?.ContainsProfanity != null)
                        return embeddedResponse.ContainsProfanity;
                }
            }
            
            // Format 3: Look for the property directly with regex
            var match = Regex.Match(jsonResponse, "\"containsProfanity\"\\s*:\\s*(true|false)");
            if (match.Success)
            {
                return match.Groups[1].Value.ToLower() == "true";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing AI response: {ex.Message}");
        }
        
        return null;
    }

    private bool ContainsObviousProfanity(string message)
    {
        // Check against the static list of profanity words
        string lowerMessage = message.ToLower();
        
        // Check for exact matches of profanity words
        foreach (var word in _profanityWords)
        {
            if (Regex.IsMatch(lowerMessage, $"\\b{Regex.Escape(word)}\\b"))
            {
                return true;
            }
        }
        
        // Check for common patterns (e.g., f**k, s**t)
        var patterns = new[]
        {
            @"f[^\w]*u[^\w]*c[^\w]*k",
            @"s[^\w]*h[^\w]*i[^\w]*t",
            @"a[^\w]*s[^\w]*s",
            @"b[^\w]*i[^\w]*t[^\w]*c[^\w]*h",
            @"d[^\w]*i[^\w]*c[^\w]*k",
            @"c[^\w]*u[^\w]*n[^\w]*t"
        };
        
        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(lowerMessage, pattern))
            {
                return true;
            }
        }
        
        return false;
    }

    // Helper class for deserializing response
    private class TextWrapper
    {
        public string Text { get; set; }
    }

    private class ProfanityDetectionResult
    {
        public bool ContainsProfanity { get; set; }
        public List<string> InappropriateTerms { get; set; }
        public string Explanation { get; set; }
        public string OriginalMessage { get; set; }
    }

    private class ModerationResult
    {
        public string OriginalMessage { get; set; }
        public string ModeratedMessage { get; set; }
        public bool WasModified { get; set; }
    }

    /// <inheritdoc />
    public string FilterProfanity(string text)
    {
        // In AI-ONLY mode, we'll always use the async version and wait for it
        return FilterProfanityAsync(text).GetAwaiter().GetResult();
    }
} 