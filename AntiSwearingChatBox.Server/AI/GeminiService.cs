using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AntiSwearingChatBox.AI;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace AntiSwearingChatBox.Server.AI
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

            Console.WriteLine($"GeminiService initialized with model: {_settings.ModelName}");
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
                Console.WriteLine($"GenerateTextAsync error: {ex.Message}");
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
                Console.WriteLine($"GenerateJsonResponseAsync error: {ex.Message}");
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        public async Task<string> ModerateChatMessageAsync(string message)
        {
            string promptTemplate =
                $"CRITICAL MODERATION TASK: Analyze the following message for ANY type of profanity, swear words, or inappropriate language with MAXIMUM SEVERITY.\n\n" +
                $"This is an anti-swearing chatbox and you MUST detect and censor ALL forms of profanity, no matter how subtle or disguised.\n\n" +
                $"DETECTION CHECKLIST - for EACH of these profanity words and their variants:\n" +
                $"- fuck, fuk, fvck, f*ck, f**k, fck, fuuck, fuuk, phuck, etc.\n" + 
                $"- shit, sh*t, sh1t, sht, sh!t, shiit, shyt, etc.\n" +
                $"- ass, a$$, a**, @ss, azz, a$, etc.\n" +
                $"- bitch, b*tch, b!tch, btch, biatch, etc.\n" +
                $"- dick, d*ck, d!ck, dck, etc.\n" +
                $"- pussy, pu$$y, pus$y, etc.\n" +
                $"- cunt, c*nt, kunt, etc.\n\n" +
                
                $"CHECK FOR THESE EVASION TECHNIQUES:\n" +
                $"1. Character substitutions: @ for a, $ for s, 0 for o, etc.\n" +
                $"2. Deliberate misspellings: fuk, phuck, etc.\n" +
                $"3. Letter spacing: f u c k, s h i t, etc.\n" +
                $"4. Character repeats: fuuuck, shiiit, etc.\n" +
                $"5. Character omissions: fk, sht, etc.\n" +
                $"6. Word fragments: fuc, shi, etc.\n" +
                $"7. Phonetic variants: fudge (for fuck), sheet (for shit), etc.\n" +
                $"8. Leet speak: f4ck, sh1t, @ss, etc.\n\n" +
                
                $"VARIANT DETECTION: For each letter in each profanity word, check if it could be replaced with ANY character that looks or sounds similar.\n" +
                $"Then check if ANY segment of the message could match these variants.\n\n" +
                
                $"MODERATION INSTRUCTIONS:\n" +
                $"- Replace each detected profanity or variant with the EXACT same number of asterisks (*)\n" +
                $"- CRITICAL: Err strongly on the side of caution - prefer false positives over missed profanity\n" +
                $"- For borderline cases, always censor\n" +
                $"- Preserve message structure and non-profane words\n\n" +
                
                $"CHECK WORD-BY-WORD: For each word in the message, analyze if it's a potential variant of ANY profanity word.\n\n" +
                
                $"Return the result in JSON format with the following structure:\n" +
                $"{{\"original\": \"original message\", \"moderated\": \"moderated message with all profanity replaced by asterisks\", \"wasModified\": true/false}}\n\n" +
                
                $"MESSAGE TO MODERATE: \"{message}\"";

            Console.WriteLine($"Sending message for moderation: \"{message}\"");
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

                // First perform direct pattern check before using AI
                if (RequestProcessor.ContainsDirectProfanity(message))
                {
                    Console.WriteLine($"Direct profanity check caught inappropriate content in: \"{message}\"");

                    // Create a direct response for profanity detection
                    var directResponse = new
                    {
                        containsProfanity = true,
                        inappropriateTerms = new[] { "detected by direct pattern matching" },
                        explanation = "Direct pattern matching detected inappropriate language",
                        originalMessage = message
                    };
                    return JsonSerializer.Serialize(directResponse);
                }

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

        /// <summary>
        /// Detects profanity with detailed explanations of all AI processing steps
        /// </summary>
        public async Task<string> DetectProfanityWithDetailsAsync(string message)
        {
            try
            {
                Console.WriteLine($"VERBOSE MODE: Checking message for profanity: \"{message}\"");

                // Create response object to track all processing steps
                var detailedResponse = new
                {
                    originalMessage = message,
                    processingSteps = new List<object>(),
                    finalResult = new { },
                    processingTimeMs = 0
                };

                // Start timing the process
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                // Record initial step
                var stepsList = new List<object>
                {
                    new
                    {
                        step = "Initialization",
                        description = "Starting profanity detection with detailed logging",
                        timestamp = DateTime.Now
                    }
                };

                bool directPatternResult = RequestProcessor.ContainsDirectProfanity(message);
                stepsList.Add(new
                {
                    step = "Direct Pattern Matching",
                    description = "Checking against known profanity patterns",
                    result = directPatternResult ? "Profanity detected" : "No profanity detected",
                    matchFound = directPatternResult,
                    timestamp = DateTime.Now
                });

                object finalResult;
                if (directPatternResult)
                {
                    stepsList.Add(new
                    {
                        step = "AI Processing",
                        description = "Skipped - Direct pattern matching already detected profanity",
                        timestamp = DateTime.Now
                    });

                    finalResult = new
                    {
                        containsProfanity = true,
                        inappropriateTerms = new[] { "detected by direct pattern matching" },
                        explanation = "Direct pattern matching detected inappropriate language",
                        detectionMethod = "Direct pattern matching",
                        originalMessage = message
                    };
                }
                else
                {
                    string enhancedPrompt = RequestProcessor.EnhancePrompt(message, "profanity");
                    stepsList.Add(new
                    {
                        step = "AI Prompt Creation",
                        description = "Creating enhanced prompt for AI model",
                        enhancedPrompt,
                        timestamp = DateTime.Now
                    });

                    stepsList.Add(new
                    {
                        step = "AI Model Inference",
                        description = "Sending request to Gemini AI model",
                        modelName = _settings.ModelName,
                        timestamp = DateTime.Now
                    });

                    string aiResponse = await GenerateJsonResponseAsync(enhancedPrompt);
                    stepsList.Add(new
                    {
                        step = "AI Response Received",
                        description = "Received raw response from AI model",
                        rawResponse = aiResponse,
                        timestamp = DateTime.Now
                    });

                    string processedResponse = RequestProcessor.ValidateAndFixResponseWithDetails(aiResponse, message, out var processingDetails);
                    stepsList.Add(new
                    {
                        step = "Response Validation",
                        description = "Validating and fixing AI response",
                        processingDetails,
                        timestamp = DateTime.Now
                    });

                    try
                    {
                        using var doc = JsonDocument.Parse(processedResponse);
                        finalResult = JsonSerializer.Deserialize<object>(processedResponse)!;
                    }
                    catch (Exception ex)
                    {
                        finalResult = new
                        {
                            error = $"Failed to parse final result: {ex.Message}",
                            originalMessage = message,
                            containsProfanity = false
                        };
                    }
                }

                // Stop timing and complete the response
                stopwatch.Stop();

                var completeResult = new
                {
                    originalMessage = message,
                    processingTimeMs = stopwatch.ElapsedMilliseconds,
                    processingSteps = stepsList,
                    finalResult
                };

                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(completeResult, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in verbose profanity detection: {ex.Message}");
                // Return error with stack trace in verbose mode
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    originalMessage = message,
                    result = new { containsProfanity = false, reason = "Error during processing" }
                }, options);
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
            string promptTemplate;
            
            if (detectedLanguage.ToLower() == "auto-detect" || detectedLanguage.ToLower() == "auto")
            {
                promptTemplate = 
                    $"You are an expert multi-language profanity filter for an anti-swearing chatbox. " +
                    $"TASK: First detect the language of the following message, then identify and censor ALL inappropriate language in that language.\n\n" +
                    
                    $"Message to moderate: \"{message}\"\n\n" +
                    
                    $"Step 1: Detect the language. Consider ALL possible languages including English, Spanish, French, German, Italian, Vietnamese, Portuguese, Russian, etc.\n" +
                    $"Step 2: Once you identify the language, identify ANY profanity, slurs, or inappropriate words in that language.\n" +
                    $"Step 3: Replace profanity with the same number of asterisks (*).\n" +
                    $"Step 4: Return a JSON object with these properties:\n" +
                    $"- \"originalMessage\": the original message you received\n" +
                    $"- \"moderatedMessage\": the message with profanity replaced by asterisks\n" +
                    $"- \"language\": the detected language name (e.g., \"English\", \"Spanish\", \"Vietnamese\")\n" +
                    $"- \"wasModified\": true if any profanity was detected and censored, false otherwise\n" +
                    $"- \"culturalContext\": brief explanation of any culturally specific profanity detected\n\n" +
                    
                    $"IMPORTANT: You MUST detect and censor profanity in ANY language. This includes:\n" +
                    $"- English: fuck, shit, ass, bitch, etc.\n" +
                    $"- Spanish: puta, mierda, joder, etc.\n" +
                    $"- French: putain, merde, etc.\n" +
                    $"- German: scheiße, arschloch, etc.\n" +
                    $"- Vietnamese: đụ, dcm, đmm, du ma, etc.\n" +
                    $"- And ALL other languages\n\n" +
                    
                    $"Even if you're only 70% sure something is profanity, you should censor it. Be thorough and conservative.";
            }
            else
            {
                promptTemplate = 
                    $"You are an expert multi-language profanity filter for an anti-swearing chatbox. " +
                    $"TASK: Moderate the following message which is in {detectedLanguage}. " +
                    $"Identify and replace ANY inappropriate language specific to {detectedLanguage} with asterisks (*).\n\n" +
                    
                    $"Message to moderate: \"{message}\"\n\n" +
                    
                    $"IMPORTANT: You MUST detect and censor ALL types of profanity in {detectedLanguage}, including:\n" +
                    $"- Common profanity/swear words\n" +
                    $"- Slurs and derogatory terms\n" +
                    $"- Sexual references\n" +
                    $"- Obfuscated profanity (like f*ck, sh!t, etc.)\n" +
                    $"- Culturally specific inappropriate terms\n\n" +
                    
                    $"Return ONLY a JSON object with the structure: " +
                    $"{{\"originalMessage\": \"{message}\", \"moderatedMessage\": \"censored message\", " +
                    $"\"language\": \"{detectedLanguage}\", \"wasModified\": true/false, " +
                    $"\"culturalContext\": \"any important cultural notes\"}}";
            }

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