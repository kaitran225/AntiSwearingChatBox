using System.Text.RegularExpressions;
using System.Text.Json;
using AntiSwearingChatBox.AI.Filter;
using AntiSwearingChatBox.Server.AI;

namespace AntiSwearingChatBox.AI.Services;


public class ProfanityFilterService : IProfanityFilter
{
    private readonly GeminiService? _geminiService;
    private const int MAX_RETRY_ATTEMPTS = 3;
    private readonly List<string> _profanityWords = new List<string> { 
        "fuck", "shit", "ass", "bitch", "dick", "cunt", "pussy", "asshole", "bastard", "piss", 
        "whore", "slut", "bullshit", "fag", "faggot", "damn", "wank", "cock", "twat", "prick"
    };

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
        
        // First check with multi-language detection (fast, non-AI approach)
        if (DetectMultiLanguageProfanity(text))
        {
            Console.WriteLine($"Multi-language profanity detected in: \"{text}\"");
            return true;
        }
        
        // Then proceed with AI detection if multi-language check doesn't find anything
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
            
            // Always use AI for moderation when available - it can handle all languages
            if (_geminiService != null)
            {
                Console.WriteLine("Using AI for multi-language profanity filtering");
                try
                {
                    // Use multi-language moderation (the AI will determine language)
                    // We pass "auto" to let the AI detect the language
                    var moderatedText = await _geminiService.ModerateMultiLanguageMessageAsync(text, "auto");
                    
                    var moderationResult = JsonSerializer.Deserialize<ModerationResult>(moderatedText);
                    
                    if (moderationResult != null && !string.IsNullOrEmpty(moderationResult.ModeratedMessage))
                    {
                        Console.WriteLine($"AI moderated multi-language message from \"{text}\" to \"{moderationResult.ModeratedMessage}\"");
                        Console.WriteLine($"Detected language: {moderationResult.Language ?? "unknown"}");
                        return moderationResult.ModeratedMessage;
                    }
                    else
                    {
                        Console.WriteLine("AI multi-language moderation produced empty or null result, trying standard moderation");
                        
                        // Fall back to standard moderation
                        moderatedText = await _geminiService.ModerateChatMessageAsync(text);
                        
                        moderationResult = JsonSerializer.Deserialize<ModerationResult>(moderatedText);
                        
                        if (moderationResult != null && !string.IsNullOrEmpty(moderationResult.ModeratedMessage))
                        {
                            Console.WriteLine($"AI moderated message from \"{text}\" to \"{moderationResult.ModeratedMessage}\"");
                            return moderationResult.ModeratedMessage;
                        }
                    }
                    
                    Console.WriteLine("All AI moderation attempts failed to produce valid results, falling back to detection");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in AI moderation: {ex.Message}, falling back to detection");
                }
            }
            else
            {
                Console.WriteLine("No AI service available, using detection-based filtering");
            }
            
            // If we reach here, AI moderation failed or no AI service available
            // Fall back to multi-language detection first
            bool containsProfanity = DetectMultiLanguageProfanity(text);
            
            // If not found by multi-language detection, try AI detection as last resort
            if (!containsProfanity && _geminiService != null)
            {
                containsProfanity = await DetectProfanityWithAIAsync(text);
            }
            
            if (containsProfanity)
            {
                Console.WriteLine("Profanity detected, applying regex filtering as fallback");
                return FilterProfanityWithRegex(text);
            }
            
            return text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in profanity filtering: {ex.Message}, applying fallback censoring");
            
            // Fallback censoring - if anything goes wrong, conservatively censor any possible bad words
            try
            {
                // Try regex filter as first fallback
                return FilterProfanityWithRegex(text);
            }
            catch
            {
                // Ultimate fallback - if regex fails completely, censor suspicious words manually
                string censored = text;
                string[] simplePatterns = { "fuck", "shit", "ass", "bitch", "dick", "cunt", "fack", "biatch", "bitach", "dcm", "du ma", "du" };
                foreach (var pattern in simplePatterns)
                {
                    if (censored.ToLower().Contains(pattern))
                    {
                        censored = censored.Replace(pattern, new string('*', pattern.Length), StringComparison.OrdinalIgnoreCase);
                    }
                }
                return censored;
            }
        }
    }

    private string FilterProfanityWithRegex(string text)
    {
        // IMPORTANT: Special direct fixes for the profanity seen in the screenshot
        if (text.Equals("dcm", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Direct match for 'dcm' message - using full replacement");
            return "***";
        }
        
        if (text.Equals("Du Ma May", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Direct match for 'Du Ma May' message - using full replacement");
            return "** ** ***";
        }
        
        string filteredText = text;
        
        Console.WriteLine($"FilterProfanityWithRegex processing: \"{text}\"");
        
        // Special handling for Vietnamese phrases first (highest priority)
        var vietnamesePhrases = new Dictionary<string, string> {
            {"du ma", "** **"},
            {"du ma may", "** ** ***"},
            {"đụ má", "** **"},
            {"đụ má mày", "** ** ***"}
        };
        
        // Add regex patterns for Vietnamese phrases with word boundaries
        var vietnamesePatterns = new[] {
            @"d[uụ]+\s+m[aá]+", // du ma
            @"d[uụ]+\s+m[aá]+\s+m[aáà][yi]+", // du ma may
            @"dcm", // Acronym
            @"đcm", // Acronym
            @"dmm", // Acronym
            @"đmm", // Acronym
            @"d\s*[uụ]\s*m\s*[aá]", // d u m a (spaced)
            @"d\s*[uụ]\s*m\s*[aá]\s*m\s*[aáà][yi]" // d u m a m a y (spaced)
        };
        
        // Specific strong handling for "Du Ma May" - direct replacement 
        // that will catch it even if it's part of other text
        string lowerText = filteredText.ToLower();
        if (lowerText.Contains("du ma may"))
        {
            Console.WriteLine("Found 'du ma may' using direct contains - forcing replacement");
            filteredText = filteredText.Replace("Du Ma May", "** ** ***", StringComparison.OrdinalIgnoreCase);
        }
        if (lowerText.Contains("du ma"))
        {
            Console.WriteLine("Found 'du ma' using direct contains - forcing replacement");
            filteredText = filteredText.Replace("Du Ma", "** **", StringComparison.OrdinalIgnoreCase);
        }
        if (lowerText.Contains("du m a m a y"))
        {
            Console.WriteLine("Found 'du m a m a y' using direct contains - forcing replacement");
            filteredText = filteredText.Replace("Du M a M a y", "** * * * * *", StringComparison.OrdinalIgnoreCase);
        }
        
        // Handle common Vietnamese acronyms with direct replacement
        if (lowerText.Contains("dcm"))
        {
            Console.WriteLine("Found 'dcm' using direct contains - forcing replacement");
            filteredText = filteredText.Replace("dcm", "***", StringComparison.OrdinalIgnoreCase);
            filteredText = filteredText.Replace("DCM", "***", StringComparison.OrdinalIgnoreCase);
            filteredText = filteredText.Replace("Dcm", "***", StringComparison.OrdinalIgnoreCase);
            filteredText = filteredText.Replace("dCm", "***", StringComparison.OrdinalIgnoreCase);
        }
        if (lowerText.Contains("đcm"))
        {
            Console.WriteLine("Found 'đcm' using direct contains - forcing replacement");
            filteredText = filteredText.Replace("đcm", "***", StringComparison.OrdinalIgnoreCase);
            filteredText = filteredText.Replace("ĐCM", "***", StringComparison.OrdinalIgnoreCase);
            filteredText = filteredText.Replace("Đcm", "***", StringComparison.OrdinalIgnoreCase);
        }
        
        // Handle single Vietnamese profanity words with context awareness
        // "du" by itself is often profanity, but can be a legitimate word in some contexts
        if (lowerText.Contains("du") || lowerText.Contains("đụ"))
        {
            // Check for common contexts where "du" is profanity
            if (Regex.IsMatch(lowerText, @"\b(du|đụ)\b", RegexOptions.IgnoreCase) && 
                !lowerText.Contains("du lịch") && // travel
                !lowerText.Contains("du học") &&  // study abroad
                !lowerText.Contains("du khách")) // tourist
            {
                Console.WriteLine("Found standalone Vietnamese profanity 'du/đụ' - replacing");
                filteredText = Regex.Replace(filteredText, @"\b(du|đụ)\b", "**", RegexOptions.IgnoreCase);
            }
            
            // These combinations are almost always profanity
            if (Regex.IsMatch(lowerText, @"(du|đụ)\s+.{1,3}\s+(m[aá]|mẹ|me|má)", RegexOptions.IgnoreCase))
            {
                Console.WriteLine("Found Vietnamese profanity pattern with 'du/đụ' + context - replacing entire phrase");
                filteredText = Regex.Replace(
                    filteredText, 
                    @"(du|đụ)\s+.{1,3}\s+(m[aá]|mẹ|me|má)",
                    match => new string('*', match.Length),
                    RegexOptions.IgnoreCase
                );
            }
        }
        
        // Also try with zero-width spaces (sometimes used to evade filters)
        if (lowerText.Contains("d c m"))
        {
            Console.WriteLine("Found 'd c m' using direct contains - forcing replacement");
            filteredText = filteredText.Replace("d c m", "* * *", StringComparison.OrdinalIgnoreCase);
        }
        
        foreach (var phrase in vietnamesePhrases)
        {
            try {
                // Case insensitive replacement
                string before = filteredText;
                filteredText = Regex.Replace(
                    filteredText,
                    phrase.Key,
                    phrase.Value,
                    RegexOptions.IgnoreCase
                );
                if (before != filteredText) {
                    Console.WriteLine($"Vietnamese phrase '{phrase.Key}' replaced: \"{before}\" -> \"{filteredText}\"");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error applying Vietnamese phrase pattern for '{phrase.Key}': {ex.Message}");
            }
        }
        
        // Apply regex patterns for Vietnamese phrases
        foreach (var pattern in vietnamesePatterns)
        {
            try {
                string before = filteredText;
                filteredText = Regex.Replace(
                    filteredText,
                    pattern,
                    match => new string('*', match.Length),
                    RegexOptions.IgnoreCase
                );
                if (before != filteredText) {
                    Console.WriteLine($"Vietnamese regex pattern '{pattern}' replaced: \"{before}\" -> \"{filteredText}\"");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error applying Vietnamese regex pattern '{pattern}': {ex.Message}");
            }
        }
        
        Console.WriteLine($"After Vietnamese filtering: \"{filteredText}\"");
        
        // First, check for standard word boundaries
        foreach (var word in _profanityWords)
        {
            try
            {
                filteredText = Regex.Replace(
                    filteredText, 
                    $"\\b{Regex.Escape(word)}\\b", 
                    match => new string('*', match.Length),
                    RegexOptions.IgnoreCase
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying regex pattern for word '{word}': {ex.Message}");
                // Continue with other patterns
            }
        }
        
        // Multi-language profanity dictionaries
        var multiLanguageProfanity = new Dictionary<string, string[]>
        {
            // Spanish profanity
            {"spanish", new[] {
                "puta", "mierda", "joder", "coño", "pendejo", "cabron", "cabrón", "chinga", "follar", "gilipollas",
                "hostia", "capullo", "carajo", "cojones", "culo", "marica", "maricon", "maricón", "polla"
            }},
            // French profanity
            {"french", new[] {
                "putain", "merde", "salope", "baise", "baiser", "connard", "enculé", "foutre", "cul", "salaud", 
                "enfoiré", "bordel", "couilles", "bite", "chier", "connasse", "con", "encule", "enculer", "pute"
            }},
            // German profanity
            {"german", new[] {
                "scheiße", "scheisse", "arschloch", "ficken", "fick", "fotze", "verpiss", "schwanz", "hurensohn",
                "hure", "wichser", "schwuchtel", "schlampe", "kacke"
            }},
            // Italian profanity
            {"italian", new[] {
                "cazzo", "fanculo", "stronzo", "merda", "puttana", "minchia", "figa", "culo", "troia", "vaffanculo", 
                "bastardo", "porca", "porca miseria"
            }},
            // Vietnamese profanity
            {"vietnamese", new[] {
                "đụ", "du", "đéo", "deo", "địt", "dit", "lồn", "lon", "cặc", "cac", "buồi", "buoi", "đ!t", "d!t",
                "dcm", "dmm", "đcm", "đmm", "vl", "đkm", "dkm"
            }},
            // Portuguese profanity
            {"portuguese", new[] {
                "puta", "caralho", "foda", "merda", "cú", "cu", "buceta", "fodido", "viado", "porra", "foder"
            }},
            // Russian profanity (transliterated)
            {"russian", new[] {
                "blyat", "blyad", "huy", "hui", "khuy", "pizda", "yebat", "yebal", "uebal", "uebat", "bliad",
                "suka", "mudak", "pizdec", "nahuy", "nahui", "ebat"
            }}
        };
        
        // Filter multi-language profanity
        foreach (var language in multiLanguageProfanity.Values)
        {
            foreach (var word in language)
            {
                try
                {
                    // Replace with word boundaries
                    filteredText = Regex.Replace(
                        filteredText, 
                        $"\\b{Regex.Escape(word)}\\b", 
                        match => new string('*', match.Length),
                        RegexOptions.IgnoreCase
                    );
                    
                    // Also replace partial matches for multi-language words
                    filteredText = Regex.Replace(
                        filteredText, 
                        $"{Regex.Escape(word)}", 
                        match => new string('*', match.Length),
                        RegexOptions.IgnoreCase
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying multi-language pattern for word '{word}': {ex.Message}");
                }
            }
        }
        
        // Handle pattern-based profanity with more variations including spaces and special characters
        var patterns = new[]
        {
            // F-word patterns with variations
            @"f+[^\w]*[vu@o0a]+[^\w]*[ck]+(?:[^\w]*[kx])?",
            @"f+[^\w]*[vu@o0a]+[^\w]*[qc]+[^\w]*[kx]?",
            @"ph+[^\w]*[vu@o0]+[^\w]*[ck]+[^\w]*[kx]?",
            @"f+[^\w]*[vu@o0a]+[^\w]*[qgc]+[^\w]*[kx]?",
            @"f[^\w]*u[^\w]*c[^\w]*k",
            @"f[^\w]*a[^\w]*[ck][^\w]*[k]?",
            
            // S-word patterns with variations
            @"sh+[^\w]*[i!1]+[^\w]*[t7]+",
            @"s+[^\w]*h+[^\w]*[i!1y]+[^\w]*[t7]+",
            @"sch+[^\w]*[i!1]+[^\w]*[t7]+",
            @"s[^\w]*h[^\w]*i[^\w]*t",
            @"s+[^\w]*h+[^\w]*[e3]+[^\w]*[i!1]+[^\w]*[t7]+",
            
            // A-word patterns with variations
            @"a+[^\w]*[s\$5z]+[^\w]*[s\$5z]+",
            @"a+[^\w]*[z5\$s][^\w]*[z5\$s]?[^\w]*h+[^\w]*[o0]+[^\w]*l+[^\w]*[e3]+",
            @"a[^\w]*s[^\w]*s",
            
            // B-word patterns with variations
            @"b+[^\w]*[i!1]+[^\w]*[t7]+[^\w]*[cç]+[^\w]*h+",
            @"b+[^\w]*[e3]+[^\w]*[a@4]+[^\w]*[cç]+[^\w]*h+",
            @"b+[^\w]*[i!1]+[^\w]*[t7]+[^\w]*[s\$5]+[^\w]*h+",
            @"b[^\w]*i[^\w]*t[^\w]*c[^\w]*h",
            @"b+[^\w]*[i!1]+[^\w]*[a@4]+[^\w]*[t7]+[^\w]*[cç]+[^\w]*h+",
            @"b+[^\w]*[i!1]+[^\w]*[t7]+[^\w]*[a@4]+[^\w]*[cç]+[^\w]*h+", // For "bitach" variants
            
            // D-word patterns with variations
            @"d+[^\w]*[i!1]+[^\w]*[cçk]+[^\w]*[kx]?",
            @"d+[^\w]*[i!1]+[^\w]*[qk]+[^\w]*[kx]?",
            @"d[^\w]*i[^\w]*c[^\w]*k",
            
            // C-word patterns with variations
            @"c+[^\w]*[vu]+[^\w]*n+[^\w]*[t7]+",
            @"k+[^\w]*[vu]+[^\w]*n+[^\w]*[t7]+",
            @"c[^\w]*u[^\w]*n[^\w]*t",
            
            // P-word patterns with variations
            @"p+[^\w]*[vu]+[^\w]*[s\$5]+[^\w]*[s\$5y]+[^\w]*[y]?",
            @"p+[^\w]*[vu]+[^\w]*[s\$5]+[^\w]*[i!1y]+",
            
            // Other offensive terms
            @"w+[^\w]*h+[^\w]*[o0]+[^\w]*r+[^\w]*[e3]+",
            @"s+[^\w]*l+[^\w]*[vu]+[^\w]*[t7]+",
            @"f+[^\w]*[a@4]+[^\w]*g+[^\w]*(?:g+[^\w]*[o0]+[^\w]*[t7]+)?",
            @"c+[^\w]*[o0]+[^\w]*[ck]+[^\w]*[kx]?",
            @"b+[^\w]*[a@4]+[^\w]*[s\$5z]+[^\w]*[t7]+[^\w]*[a@4]+[^\w]*r+[^\w]*d+",
            @"tw+[^\w]*[a@4]+[^\w]*[t7]+",
            
            // Multi-language patterns (Spanish)
            @"p+[^\w]*u+[^\w]*t+[^\w]*a+", // puta
            @"c+[^\w]*o+[^\w]*ñ+[^\w]*o+", // coño
            @"j+[^\w]*o+[^\w]*d+[^\w]*e+[^\w]*r+", // joder
            
            // Multi-language patterns (French)
            @"p+[^\w]*u+[^\w]*t+[^\w]*a+[^\w]*i+[^\w]*n+", // putain
            @"m+[^\w]*e+[^\w]*r+[^\w]*d+[^\w]*e+", // merde
            
            // Multi-language patterns (German)
            @"s+[^\w]*c+[^\w]*h+[^\w]*e+[^\w]*i+[^\w]*[sß]+[^\w]*[eə]+", // scheiße/scheisse
            @"f+[^\w]*i+[^\w]*c+[^\w]*k+[^\w]*e+[^\w]*n+", // ficken
            
            // Multi-language patterns (Vietnamese)
            @"đ+[^\w]*[uụ]+", // đụ
            @"đ+[^\w]*[iị]+[^\w]*[tţ]+", // địt
            @"l+[^\w]*[oồ]+[^\w]*n+", // lồn
            
            // Multi-language patterns (Russian transliterated)
            @"b+[^\w]*l+[^\w]*y+[^\w]*a+[^\w]*[td]+", // blyat/blyad
            @"p+[^\w]*i+[^\w]*z+[^\w]*d+[^\w]*a+", // pizda
            @"s+[^\w]*u+[^\w]*k+[^\w]*a+" // suka
        };
        
        foreach (var pattern in patterns)
        {
            try
            {
                filteredText = Regex.Replace(
                    filteredText, 
                    pattern, 
                    match => new string('*', match.Length),
                    RegexOptions.IgnoreCase
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying regex pattern '{pattern}': {ex.Message}");
                // Continue with other patterns
            }
        }
        
        // Handle common leet speak substitutions after replacing broader patterns
        var leetSpeakMap = new Dictionary<string, string>()
        {
            {"@", "a"}, {"4", "a"}, {"$", "s"}, {"5", "s"}, {"0", "o"}, 
            {"1", "i"}, {"!", "i"}, {"3", "e"}, {"7", "t"}, {"+", "t"},
            {"ph", "f"}, {"vv", "w"}, {"v", "u"}, {"k", "c"}, {"kk", "ck"}
        };
        
        // Create a normalized version of the text to check for hidden profanity
        string normalizedText = filteredText;
        foreach (var pair in leetSpeakMap)
        {
            normalizedText = normalizedText.Replace(pair.Key, pair.Value);
        }
        
        // Check normalized text for profanity that might have been missed
        foreach (var word in _profanityWords)
        {
            try
            {
                int startIndex = 0;
                while ((startIndex = normalizedText.IndexOf(word, startIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
                {
                    // Get the corresponding segment from the original filtered text
                    int originalLength = word.Length;
                    
                    // Ensure we don't go out of bounds
                    if (startIndex + originalLength <= filteredText.Length)
                    {
                        // Replace the segment in the filtered text with asterisks
                        string segment = filteredText.Substring(startIndex, originalLength);
                        filteredText = filteredText.Remove(startIndex, originalLength)
                                  .Insert(startIndex, new string('*', originalLength));
                    }
                    
                    startIndex += originalLength;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking normalized text for word '{word}': {ex.Message}");
                // Continue with other words
            }
        }
        
        // Final pass: check for any non-Latin multi-language profanity that may have been missed
        string normalizedNoAccents = RemoveDiacritics(filteredText.ToLower());
        foreach (var language in multiLanguageProfanity.Values)
        {
            foreach (var word in language)
            {
                string normalizedWord = RemoveDiacritics(word.ToLower());
                try
                {
                    int startIndex = 0;
                    while ((startIndex = normalizedNoAccents.IndexOf(normalizedWord, startIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
                    {
                        int originalLength = normalizedWord.Length;
                        
                        if (startIndex + originalLength <= filteredText.Length)
                        {
                            filteredText = filteredText.Remove(startIndex, originalLength)
                                      .Insert(startIndex, new string('*', originalLength));
                            normalizedNoAccents = normalizedNoAccents.Remove(startIndex, originalLength)
                                             .Insert(startIndex, new string('*', originalLength));
                        }
                        
                        startIndex += originalLength;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking normalized multi-language text for word '{word}': {ex.Message}");
                }
            }
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
        
        // Enhanced patterns to catch more sophisticated evasion techniques
        var suspiciousPatterns = new[]
        {
            // F-word patterns
            @"f\W*[vuo@40]",                  // Starts with f + vowel (likely profanity)
            @"[vuo@40]\W*c?k",                // Ends with c/k pattern (likely profanity)
            @"ph\W*[vuo@40]",                 // Ph variant
            @"ef+\W*ing",                     // Effing variant
            @"f+\W*[vuo@40]+\W*[ckg]+",       // Basic f**k pattern
            
            // S-word patterns
            @"sh\W*[i1!]",                    // Starts with shi pattern
            @"sh\W*[i1!]+\W*[t7]",            // Sh*t pattern
            @"sh[i1!]+[t7]",                  // Compact shit
            @"[5s]+\W*h\W*[i1!]+\W*[t7]",     // S h i t pattern
            
            // A-word patterns
            @"a\W*[sz5$]\W*[sz5$]",           // a*s*s pattern
            @"[sz5$]\W*h[o0]l",               // Asshole variant
            @"[a@4]\W*[sz5$]+\W*h[o0]l",      // A**hole pattern
            
            // B-word patterns
            @"b\W*[i1!]\W*t\W*c",             // b*i*t*c pattern
            @"b\W*[i1!]+\W*[a@4]?\W*t?c?h",   // Bitch variants
            @"b[i1!]+[t7]ch",                 // Compact bitch
            
            // D-word patterns
            @"d\W*[i1!]\W*[ck]",              // Dick pattern
            @"d[i1!]+[ck]",                   // Compact dick
            
            // C-word patterns
            @"c\W*u\W*n\W*t",                 // c*u*n*t pattern
            @"[ck]\W*[uo0]n",                 // Cunt beginnings
            
            // P-word patterns  
            @"p\W*[uo0]\W*s\W*[sy]",          // p*u*s*s*y pattern
            @"p[uo0]+[s5$]+[yi1!]",           // Compact pussy
            
            // Composite patterns for compound profanity
            @"m[o0]\W*th[e3]r\W*f",           // Motherf***
            @"g[o0]\W*f[uo0]",                // Go f*** yourself pattern
        };
        
        foreach (var pattern in suspiciousPatterns)
        {
            if (Regex.IsMatch(lowerMessage, pattern))
            {
                return true;
            }
        }
        
        // Check for potential obfuscation through spacing
        string noSpaceMessage = Regex.Replace(lowerMessage, @"\s+", "");
        foreach (var word in _profanityWords)
        {
            if (noSpaceMessage.Contains(word))
            {
                return true;
            }
        }
        
        // Check for character substitution patterns (e.g., replacing 'a' with '@')
        string normalizedMessage = lowerMessage
            .Replace("@", "a")
            .Replace("4", "a")
            .Replace("$", "s")
            .Replace("5", "s")
            .Replace("0", "o")
            .Replace("1", "i")
            .Replace("!", "i")
            .Replace("3", "e")
            .Replace("7", "t")
            .Replace("+", "t")
            .Replace("(", "c")
            .Replace(")", "o");
            
        foreach (var word in _profanityWords)
        {
            if (normalizedMessage.Contains(word))
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
        
        // First check with direct word boundaries
        foreach (var word in _profanityWords)
        {
            if (Regex.IsMatch(lowerMessage, $"\\b{Regex.Escape(word)}\\b"))
            {
                return true;
            }
        }
        
        // Then check with standard spacing patterns
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
        
        // Finally, check normalized text to catch more obfuscated variants
        string normalizedText = NormalizeTextForProfanityDetection(message);
        foreach (var word in _profanityWords)
        {
            if (normalizedText.Contains(word))
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Normalizes text by removing spacing, replacing common character substitutions,
    /// and standardizing variations to improve profanity detection
    /// </summary>
    public static string NormalizeTextForProfanityDetection(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        // Convert to lowercase
        string result = input.ToLower();
        
        // Remove all whitespace
        result = Regex.Replace(result, @"\s+", "");
        
        // Replace repeating characters with single instances
        result = Regex.Replace(result, @"(.)\1+", "$1");
        
        // Replace common character substitutions
        var substitutions = new Dictionary<string, string>
        {
            // Common character substitutions
            {"@", "a"}, {"4", "a"}, {"^", "a"}, {"α", "a"}, {"ä", "a"}, {"å", "a"}, {"à", "a"}, {"á", "a"}, {"â", "a"}, {"ą", "a"},
            {"8", "b"}, {"ẞ", "b"}, {"ß", "b"}, {"ь", "b"}, 
            {"(", "c"}, {"{", "c"}, {"[", "c"}, {"<", "c"}, {"¢", "c"}, {"©", "c"}, {"ç", "c"}, {"ć", "c"},
            {"đ", "d"}, {"ð", "d"},
            {"3", "e"}, {"€", "e"}, {"є", "e"}, {"ë", "e"}, {"ê", "e"}, {"è", "e"}, {"é", "e"}, {"ę", "e"}, {"&", "e"},
            {"ph", "f"}, {"ƒ", "f"}, {"ф", "f"},
            {"9", "g"}, {"ğ", "g"}, {"ǵ", "g"}, 
            {"#", "h"}, {"ħ", "h"}, {"ĥ", "h"},
            {"1", "i"}, {"!", "i"}, {"|", "i"}, {"ï", "i"}, {"î", "i"}, {"í", "i"}, {"ì", "i"},
            {"j", "j"},
            {"k", "k"},
            {"£", "l"}, {"ł", "l"},
            {"m", "m"}, {"ɱ", "m"},
            {"n", "n"}, {"ñ", "n"}, {"ń", "n"}, {"ň", "n"},
            {"0", "o"}, {"()", "o"}, {"[]", "o"}, {"<>", "o"}, {"ø", "o"}, {"ö", "o"}, {"ô", "o"}, {"ò", "o"}, {"ó", "o"}, {"õ", "o"},
            {"p", "p"}, {"þ", "p"}, {"¶", "p"}, {"ρ", "p"},
            {"q", "q"},
            {"r", "r"}, {"®", "r"}, {"ŕ", "r"},
            {"5", "s"}, {"$", "s"}, {"z", "s"}, {"§", "s"}, {"ŝ", "s"}, {"š", "s"}, {"ś", "s"},
            {"7", "t"}, {"+", "t"}, {"†", "t"}, {"τ", "t"}, {"ţ", "t"}, {"ť", "t"},
            {"µ", "u"}, {"ü", "u"}, {"û", "u"}, {"ù", "u"}, {"ú", "u"}, {"ū", "u"},
            {"√", "v"}, {"ν", "v"}, {"υ", "v"},
            {"w", "w"}, {"vv", "w"}, {"ŵ", "w"}, {"ẃ", "w"},
            {"×", "x"}, {"χ", "x"},
            {"y", "y"}, {"¥", "y"}, {"ý", "y"},
            {"ž", "z"}, {"ź", "z"}, {"ż", "z"}
        };
        
        // Apply all substitutions
        foreach (var pair in substitutions)
        {
            result = result.Replace(pair.Key, pair.Value);
        }
        
        // Remove non-alphanumeric characters
        result = Regex.Replace(result, @"[^a-z0-9]", "");
        
        return result;
    }

    /// <summary>
    /// Detects profanity in multiple languages
    /// </summary>
    public bool DetectMultiLanguageProfanity(string message)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        // Check with standard English profanity detection first
        if (ContainsObviousProfanity(message))
            return true;
            
        // Multi-language profanity dictionaries
        var multiLanguageProfanity = new Dictionary<string, string[]>
        {
            // Spanish profanity
            {"spanish", new[] {
                "puta", "mierda", "joder", "coño", "pendejo", "cabron", "cabrón", "chinga", "follar", "gilipollas",
                "hostia", "capullo", "carajo", "cojones", "culo", "marica", "maricon", "maricón", "polla"
            }},
            // French profanity
            {"french", new[] {
                "putain", "merde", "salope", "baise", "baiser", "connard", "enculé", "foutre", "cul", "salaud", 
                "enfoiré", "bordel", "couilles", "bite", "chier", "connasse", "con", "encule", "enculer", "pute"
            }},
            // German profanity
            {"german", new[] {
                "scheiße", "scheisse", "arschloch", "ficken", "fick", "fotze", "verpiss", "schwanz", "hurensohn",
                "hure", "wichser", "schwuchtel", "schlampe", "kacke"
            }},
            // Italian profanity
            {"italian", new[] {
                "cazzo", "fanculo", "stronzo", "merda", "puttana", "minchia", "figa", "culo", "troia", "vaffanculo", 
                "bastardo", "porca", "porca miseria"
            }},
            // Vietnamese profanity
            {"vietnamese", new[] {
                "đụ", "du", "đéo", "deo", "địt", "dit", "lồn", "lon", "cặc", "cac", "buồi", "buoi", "đ!t", "d!t",
                "dcm", "dmm", "đcm", "đmm", "vl", "đkm", "dkm"
            }},
            // Portuguese profanity
            {"portuguese", new[] {
                "puta", "caralho", "foda", "merda", "cú", "cu", "buceta", "fodido", "viado", "porra", "foder"
            }},
            // Russian profanity (transliterated)
            {"russian", new[] {
                "blyat", "blyad", "huy", "hui", "khuy", "pizda", "yebat", "yebal", "uebal", "uebat", "bliad",
                "suka", "mudak", "pizdec", "nahuy", "nahui", "ebat"
            }}
        };

        string lowerMessage = message.ToLower();
        
        // Check each language's profanity list
        foreach (var language in multiLanguageProfanity.Values)
        {
            foreach (var word in language)
            {
                // Check for exact matches with word boundaries
                if (Regex.IsMatch(lowerMessage, $"\\b{Regex.Escape(word)}\\b", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine($"Multi-language profanity detected: {word}");
                    return true;
                }
                
                // Check if the word is contained within the message
                if (lowerMessage.Contains(word))
                {
                    Console.WriteLine($"Multi-language profanity contained: {word}");
                    return true;
                }
            }
        }
        
        // Check for profanity with diacritics removed
        string normalizedMessage = RemoveDiacritics(lowerMessage);
        
        foreach (var language in multiLanguageProfanity.Values)
        {
            foreach (var word in language)
            {
                string normalizedWord = RemoveDiacritics(word);
                if (normalizedMessage.Contains(normalizedWord))
                {
                    Console.WriteLine($"Multi-language profanity detected after normalization: {word}");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Removes diacritics (accent marks) from characters
    /// </summary>
    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
            
        string normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (char c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
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
        public string? Language { get; set; }
    }

    public string FilterProfanity(string text)
    {
        return FilterProfanityAsync(text).GetAwaiter().GetResult(); // For AI-ONLY mode
    }
} 