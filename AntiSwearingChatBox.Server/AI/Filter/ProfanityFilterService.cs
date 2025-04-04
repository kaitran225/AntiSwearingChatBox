using System.Text.RegularExpressions;
using System.Text.Json;
using AntiSwearingChatBox.AI.Filter;
using AntiSwearingChatBox.Server.AI;

namespace AntiSwearingChatBox.AI.Services;


public class ProfanityFilterService : IProfanityFilter
{
    private readonly GeminiService? _geminiService;
    private const int MAX_RETRY_ATTEMPTS = 3;
    private readonly List<string> _profanityWords = new List<string> { "fuck", "shit", "ass", "bitch", "dick", "cunt" };

    public ProfanityFilterService(GeminiService? geminiService = null)
    {
        _geminiService = geminiService;
        
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

    public async Task<bool> ContainsProfanityAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;
        
        return await DetectProfanityWithAIAsync(text);
    }

    public async Task<string> FilterTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
            
        if (_geminiService == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: GeminiService is null, cannot perform AI text moderation");
            System.Console.WriteLine("ERROR: GeminiService is null, cannot perform AI text moderation");
            throw new System.InvalidOperationException("GeminiService is required for AI-ONLY mode");
        }
            
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
                    await Task.Delay(500 * (attempt + 1));
                }
            }
        }
        
        string censored = new string('*', text.Length);
        System.Diagnostics.Debug.WriteLine($"All AI attempts failed to moderate, conservatively censoring text: \"{text}\" → \"{censored}\"");
        System.Console.WriteLine($"All AI attempts failed to moderate, conservatively censoring text: \"{text}\" → \"{censored}\"");
        return censored;
    }
    public async Task<string> FilterProfanityAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        try
        {
            Console.WriteLine($"Checking message for profanity: \"{text}\"");
            
            if (_geminiService == null)
            {
                Console.WriteLine("No AI service available, using regex-based filtering");
                return FilterProfanityWithRegex(text);
            }
            
            bool containsProfanity = await DetectProfanityWithAIAsync(text);
            
            if (containsProfanity)
            {
                try
                {
                    var moderatedText = await _geminiService.ModerateChatMessageAsync(text);
                    
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
                
                return FilterProfanityWithRegex(text);
            }
            
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
        foreach (var word in _profanityWords)
        {
            filteredText = Regex.Replace(
                filteredText, 
                $"\\b{Regex.Escape(word)}\\b", 
                match => new string('*', match.Length),
                RegexOptions.IgnoreCase
            );
        }
        
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
                
            
                var jsonResponse = await _geminiService?.DetectProfanityAsync(message)!;
                Console.WriteLine($"Raw AI response: {jsonResponse}");
                
                bool? containsProfanity = ExtractContainsProfanityFromResponse(jsonResponse);
                
                if (containsProfanity.HasValue)
                {
                    if (!containsProfanity.Value)
                    {
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
        
        if (currentAttempt >= maxAttempts || !useAI)
        {
            Console.WriteLine($"Falling back to regex-based profanity detection for: \"{message}\"");
            return ContainsObviousProfanity(message);
        }
        
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
            if (jsonResponse.TrimStart().StartsWith("{"))
            {
                var directResponse = JsonSerializer.Deserialize<ProfanityDetectionResult>(jsonResponse);
                if (directResponse?.ContainsProfanity != null)
                    return directResponse.ContainsProfanity;
            }
            
            if (jsonResponse.Contains("\"text\""))
            {
                var wrapper = JsonSerializer.Deserialize<TextWrapper>(jsonResponse);
                if (!string.IsNullOrEmpty(wrapper?.Text))
                {
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
                    var embeddedResponse = JsonSerializer.Deserialize<ProfanityDetectionResult>(text);
                    if (embeddedResponse?.ContainsProfanity != null)
                        return embeddedResponse.ContainsProfanity;
                }
            }
            
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
        string lowerMessage = message.ToLower();
        
        foreach (var word in _profanityWords)
        {
            if (Regex.IsMatch(lowerMessage, $"\\b{Regex.Escape(word)}\\b"))
            {
                return true;
            }
        }
        
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

    private class TextWrapper
    {
        public string? Text { get; set; }
    }

    private class ProfanityDetectionResult
    {
        public bool ContainsProfanity { get; set; }
        public List<string>? InappropriateTerms { get; set; }
        public string? Explanation { get; set; }
        public string? OriginalMessage { get; set; }
    }

    private class ModerationResult
    {
        public string? OriginalMessage { get; set; }
        public string? ModeratedMessage { get; set; }
        public bool WasModified { get; set; }
    }

    public string FilterProfanity(string text)
    {
        return FilterProfanityAsync(text).GetAwaiter().GetResult(); // For AI-ONLY mode
    }
} 