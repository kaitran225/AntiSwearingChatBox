using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntiSwearingChatBox.WPF.Models.Api;

namespace AntiSwearingChatBox.WPF.Services.Api
{
    public interface IApiService
    {
        // User information
        UserModel? CurrentUser { get; }
        
        // Authentication
        Task<AuthResponse> LoginAsync(string username, string password);
        Task<AuthResponse> RegisterAsync(string username, string email, string password);
        
        // Chat Threads
        Task<List<ChatThread>> GetThreadsAsync();
        Task<ChatThread> CreateThreadAsync(string name);
        Task<ChatThread> CreatePrivateChatAsync(int otherUserId, string title = "");
        Task<List<UserModel>> GetUsersAsync();
        Task<bool> JoinThreadAsync(int threadId);
        
        // Messages
        Task<List<ChatMessage>> GetMessagesAsync(int threadId);
        Task<List<ChatMessage>> GetMessagesAsync(int threadId, DateTime? since);
        Task<List<ChatMessage>> GetMessagesAsync(int threadId, int limit);
        Task<ChatMessage> SendMessageAsync(int threadId, string content);
        
        // Real-time connection
        Task<bool> ConnectToHubAsync();
        Task DisconnectFromHubAsync();
        Task<bool> IsHubConnectedAsync();
        Task<bool> JoinThreadChatGroupAsync(int threadId);
        Task<bool> LeaveThreadChatGroupAsync(int threadId);
        
        // Events
        event Action<ChatMessage> OnMessageReceived;
        event Action<ChatThread> OnThreadCreated;
        event Action<int, string> OnUserJoinedThread;
        event Action<int, int, bool> OnThreadInfoUpdated; // threadId, swearingScore, isClosed
        event Action<int, string> OnThreadClosed; // threadId, reason
        event Action<bool> OnConnectionStateChanged; // isConnected
        
        // AI Services
        Task<string> GenerateTextAsync(string prompt);
        Task<ModerationResult> ModerateChatMessageAsync(string message);
        Task<ProfanityDetectionResult> DetectProfanityAsync(string message, bool verbose = false);
        Task<ContextFilterResult> ContextAwareFilteringAsync(string message, string conversationContext);
        Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string message);
        Task<DeescalationResult> GenerateDeescalationResponseAsync(string message);
        Task<MessageHistoryAnalysisResult> ReviewMessageHistoryAsync(List<string> messages);
        Task<AlternativeMessageResult> SuggestAlternativeMessageAsync(string message);
        Task<MultiLanguageModerationResult> ModerateMultiLanguageMessageAsync(string message, string language);
        Task<UserReputationResult> AnalyzeUserReputationAsync(List<string> messages, int priorWarnings);

        /// <summary>
        /// Updates the swearing score for a thread
        /// </summary>
        Task<bool> UpdateThreadSwearingScoreAsync(int threadId, int score);

        /// <summary>
        /// Marks a thread as closed (due to excessive swearing or other reasons)
        /// </summary>
        Task<bool> CloseThreadAsync(int threadId);

        /// <summary>
        /// Gets participants for a specific thread
        /// </summary>
        Task<List<ParticipantDto>> GetThreadParticipantsAsync(int threadId);

        /// <summary>
        /// Gets the current swearing score for a thread via direct API call (not SignalR)
        /// </summary>
        Task<int> GetThreadSwearingScoreAsync(int threadId);

        /// <summary>
        /// Checks if a thread is closed via direct API call (not SignalR)
        /// </summary>
        Task<bool> IsThreadClosedAsync(int threadId);
    }
} 