using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.WPF.Models.Api
{
    /// <summary>
    /// Result of a chat message moderation by AI
    /// </summary>
    public class ModerationResult
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string ModeratedMessage { get; set; } = string.Empty;
        public bool WasModified { get; set; }
    }
    
    /// <summary>
    /// Result of profanity detection by AI
    /// </summary>
    public class ProfanityDetectionResult
    {
        public bool ContainsProfanity { get; set; }
        public List<string> InappropriateTerms { get; set; } = new List<string>();
        public string Explanation { get; set; } = string.Empty;
        public string OriginalMessage { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of context-aware filtering by AI
    /// </summary>
    public class ContextFilterResult
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string ModeratedMessage { get; set; } = string.Empty;
        public bool WasModified { get; set; }
        public string ContextualExplanation { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of sentiment and toxicity analysis by AI
    /// </summary>
    public class SentimentAnalysisResult
    {
        public int SentimentScore { get; set; } // 1-10, 10 being most positive
        public string ToxicityLevel { get; set; } = string.Empty; // none, low, medium, high
        public List<string> Emotions { get; set; } = new List<string>();
        public bool RequiresIntervention { get; set; }
        public string InterventionReason { get; set; } = string.Empty;
        public string Analysis { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of generating a de-escalation response by AI
    /// </summary>
    public class DeescalationResult
    {
        public string HarmfulMessage { get; set; } = string.Empty;
        public string DeescalationResponse { get; set; } = string.Empty;
        public string ResponseStrategy { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of message history analysis by AI
    /// </summary>
    public class MessageHistoryAnalysisResult
    {
        public int MessageCount { get; set; }
        public List<FlaggedMessage> FlaggedMessages { get; set; } = new List<FlaggedMessage>();
        public string OverallAssessment { get; set; } = string.Empty;
        public List<string> RecommendedActions { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// A message flagged by AI for inappropriate content
    /// </summary>
    public class FlaggedMessage
    {
        public int Index { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of suggesting an alternative for an inappropriate message by AI
    /// </summary>
    public class AlternativeMessageResult
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string SuggestedAlternative { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of moderating a message in a specific language by AI
    /// </summary>
    public class MultiLanguageModerationResult
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string ModeratedMessage { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool WasModified { get; set; }
        public string CulturalContext { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of analyzing a user's reputation by AI
    /// </summary>
    public class UserReputationResult
    {
        public int ReputationScore { get; set; } // 1-100
        public List<string> BehaviorPatterns { get; set; } = new List<string>();
        public string Trustworthiness { get; set; } = string.Empty; // low/medium/high
        public List<string> RecommendedActions { get; set; } = new List<string>();
        public string Analysis { get; set; } = string.Empty;
    }
} 