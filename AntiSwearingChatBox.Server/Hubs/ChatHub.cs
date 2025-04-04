using AntiSwearingChatBox.AI.Filter;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using AntiSwearingChatBox.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace AntiSwearingChatBox.Server.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, UserConnection> _userConnections = new();
    private readonly IProfanityFilter _profanityFilter;
    private readonly IMessageHistoryService _messageHistoryService;
    private readonly IChatThreadService _chatThreadService;
    private readonly IUserService _userService;
    private readonly IThreadParticipantService _threadParticipantService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IProfanityFilter profanityFilter, 
        IMessageHistoryService messageHistoryService,
        IChatThreadService chatThreadService,
        IUserService userService,
        IThreadParticipantService threadParticipantService,
        IServiceProvider serviceProvider,
        ILogger<ChatHub> logger)
    {
        _profanityFilter = profanityFilter;
        _messageHistoryService = messageHistoryService;
        _chatThreadService = chatThreadService;
        _userService = userService;
        _threadParticipantService = threadParticipantService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendMessage(int threadId, string message, int userId, string username)
    {
        try
        {
            Console.WriteLine($"Attempting to send message to thread {threadId} from user {userId}");
            
            // Authenticate the user first
            bool isAuth = await AuthenticateUser(threadId, userId, username);
            if (!isAuth)
            {
                return;
            }
            
            // Check if the thread is closed due to excessive swearing
            var thread = _chatThreadService.GetById(threadId);
            if (thread == null)
            {
                // Thread doesn't exist
                await Clients.Caller.SendAsync("ReceiveSystemMessage", 
                    "Error", "The conversation doesn't exist.");
                return;
            }
            
            // Check if the thread is closed
            if (thread.IsClosed)
            {
                // Thread is closed, don't allow new messages
                await Clients.Caller.SendAsync("ReceiveSystemMessage", 
                    "Thread Closed", "This conversation has been closed due to excessive swearing. You can no longer send messages.");
                return;
            }
            
            // Continue with the rest of the original method...
            // Check for profanity if moderation is enabled
            string filteredMessage = message;
            bool containsProfanity = false;
            
            if (thread.ModerationEnabled)
            {
                try
                {
                    // Perform initial profanity check using the filter service
                    filteredMessage = await _profanityFilter.FilterProfanityAsync(message);
                    containsProfanity = filteredMessage != message;
                    
                    // If profanity is detected, we might want to perform deeper analysis
                    if (containsProfanity)
                    {
                        // Get the GeminiService from DI
                        var geminiService = _serviceProvider.GetService<GeminiService>();
                        if (geminiService != null)
                        {
                            try
                            {
                                // Get sentiment analysis to check severity
                                var analysisResponse = await geminiService.AnalyzeSentimentAsync(message);
                                var sentimentAnalysis = JsonSerializer.Deserialize<SentimentAnalysisResult>(analysisResponse);
                                
                                // Check if the message requires intervention (complete rejection)
                                if (sentimentAnalysis?.RequiresIntervention == true)
                                {
                                    // Send a private message back to the sender only
                                    await Clients.Caller.SendAsync("ReceiveSystemMessage", 
                                        "Message Rejected", 
                                        $"Your message was not sent because it violates our community guidelines. Reason: {sentimentAnalysis.InterventionReason}");
                                    
                                    // Log the rejected message
                                    _logger.LogWarning($"Message rejected in ChatHub: UserId={userId}, ThreadId={threadId}, Reason={sentimentAnalysis.InterventionReason}");
                                    
                                    // Exit the method without sending the message
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in ChatHub AI analysis: {ex.Message}");
                                // Continue with basic filtering if AI analysis fails
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in ChatHub profanity filter: {ex.Message}");
                    // Use original message if filtering fails
                }
            }
            
            // Proceed with normal message processing if not rejected by pre-screening
            bool wasModified = containsProfanity;
            
            if (wasModified)
            {
                // Send a private warning to the user
                await Clients.Caller.SendAsync("PrivateMessage", "System", "Your message contained inappropriate language and was filtered.");
            }
            
            // Store message in database
            try
            {
                var messageHistory = new MessageHistory
                {
                    ThreadId = threadId,
                    UserId = userId,
                    OriginalMessage = message,
                    ModeratedMessage = filteredMessage,
                    WasModified = wasModified,
                    CreatedAt = DateTime.UtcNow
                };
                
                var result = _messageHistoryService.Add(messageHistory);
                if (!result.success)
                {
                    // Log the error but don't affect user experience
                    System.Diagnostics.Debug.WriteLine($"Failed to save message: {result.message}");
                    await Clients.Caller.SendAsync("Error", "Failed to save message to database.");
                    return;
                }
                
                // Update the last message timestamp for the thread
                if (thread != null)
                {
                    thread.LastMessageAt = DateTime.UtcNow;
                    _chatThreadService.Update(thread);
                }
            }
            catch (Exception ex)
            {
                // Log the error and notify the user
                System.Diagnostics.Debug.WriteLine($"Exception saving message: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "An error occurred while saving your message.");
                return;
            }
            
            // Broadcast filtered message to all clients in this thread
            await Clients.Group($"thread_{threadId}").SendAsync(
                "ReceiveMessage", 
                username, 
                filteredMessage, 
                userId, 
                DateTime.UtcNow, 
                threadId, 
                message,  // Original message 
                wasModified  // Flag indicating if message was modified
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SendMessage: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "An error occurred while processing your request.");
        }
    }

    public async Task JoinChat(string username, int userId)
    {
        if (userId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid user ID. Please log in first.");
            return;
        }
        
        // Verify the user exists in the database
        var user = _userService.GetById(userId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("Error", "User not found. Please log in again.");
            return;
        }
        
        // Check if username contains profanity
        if (await _profanityFilter.ContainsProfanityAsync(username))
        {
            await Clients.Caller.SendAsync("Error", "Username contains inappropriate language. Please choose another username.");
            return;
        }

        // Store connection info
        _userConnections[Context.ConnectionId] = new UserConnection { Username = username, UserId = userId };
        
        // Add to general chat group
        await Groups.AddToGroupAsync(Context.ConnectionId, "thread_1");
        
        // Get all threads the user is a participant in
        var userThreads = _threadParticipantService.GetByUserId(userId);
        foreach (var thread in userThreads)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"thread_{thread.ThreadId}");
        }
        
        // Notify others of join
        await Clients.Others.SendAsync("UserJoined", username, userId);
        
        // Confirm join to caller
        await Clients.Caller.SendAsync("JoinConfirmed", username, userId);
        
        // Send the list of connected users to the new client
        await Clients.Caller.SendAsync("UserList", _userConnections.Values.Select(c => new { c.Username, c.UserId }).ToList());
        
        // Send the list of all users to the client for contacts
        var allUsers = _userService.GetAll();
        await Clients.Caller.SendAsync("AllUsers", allUsers.Where(u => u.IsActive).Select(u => new 
        {
            Id = u.UserId.ToString(),
            Name = u.Username,
            LastMessage = "", 
            LastMessageTime = "",
            Status = u.IsActive ? "Online" : "Offline"
        }).ToList());
        
        // Send recent messages from default thread
        try
        {
            var recentMessages = _messageHistoryService.GetByThreadId(1)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .OrderBy(m => m.CreatedAt);
                
            foreach (var msg in recentMessages)
            {
                var sender = _userService.GetById(msg.UserId)?.Username ?? "Unknown User";
                await Clients.Caller.SendAsync(
                    "ReceiveMessage", 
                    sender, 
                    msg.WasModified ? msg.ModeratedMessage : msg.OriginalMessage, 
                    msg.UserId,
                    msg.CreatedAt,
                    msg.ThreadId,
                    msg.OriginalMessage,  // Original message
                    msg.WasModified       // Flag indicating if message was modified
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading message history: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Failed to load message history.");
        }
    }
    
    public async Task JoinThread(int threadId)
    {
        if (!_userConnections.TryGetValue(Context.ConnectionId, out var connection))
        {
            await Clients.Caller.SendAsync("Error", "You must join the chat before joining a thread.");
            return;
        }
        
        // Verify the thread exists
        var thread = _chatThreadService.GetById(threadId);
        if (thread == null)
        {
            await Clients.Caller.SendAsync("Error", "Thread not found.");
            return;
        }
        
        // Verify user is a participant if it's a private thread
        if (thread.IsPrivate)
        {
            var participants = _threadParticipantService.GetByThreadId(threadId);
            if (!participants.Any(p => p.UserId == connection.UserId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a participant in this thread.");
                return;
            }
        }
        
        // Add user to thread group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"thread_{threadId}");
        
        // Load thread history and send to caller
        try
        {
            var messages = _messageHistoryService.GetByThreadId(threadId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .OrderBy(m => m.CreatedAt);
                
            foreach (var msg in messages)
            {
                var sender = _userService.GetById(msg.UserId)?.Username ?? "Unknown User";
                await Clients.Caller.SendAsync(
                    "ReceiveMessage", 
                    sender, 
                    msg.WasModified ? msg.ModeratedMessage : msg.OriginalMessage, 
                    msg.UserId,
                    msg.CreatedAt,
                    msg.ThreadId,
                    msg.OriginalMessage,  // Original message
                    msg.WasModified       // Flag indicating if message was modified
                );
            }
            
            // Notify caller that thread loading is complete
            await Clients.Caller.SendAsync("ThreadJoined", threadId, thread.Title);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading thread history: {ex.Message}");
            await Clients.Caller.SendAsync("Error", "Failed to load thread history.");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_userConnections.TryGetValue(Context.ConnectionId, out var connection))
        {
            _userConnections.Remove(Context.ConnectionId);
            await Clients.Others.SendAsync("UserLeft", connection.Username);
        }

        await base.OnDisconnectedAsync(exception);
    }
    
    private class UserConnection
    {
        public string Username { get; set; } = "";
        public int UserId { get; set; }
    }

    private async Task<bool> AuthenticateUser(int threadId, int userId, string username)
    {
        try
        {
            // Verify the thread exists
            var thread = _chatThreadService.GetById(threadId);
            if (thread == null)
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", 
                    "Error", "The conversation doesn't exist.");
                return false;
            }
            
            // Verify user is a participant in this thread
            var participants = _threadParticipantService.GetByThreadId(threadId);
            if (!participants.Any(p => p.UserId == userId))
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", 
                    "Error", "You are not a participant in this conversation.");
                return false;
            }
            
            // Check for username profanity
            if (await _profanityFilter.ContainsProfanityAsync(username))
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", 
                    "Error", "Your username contains inappropriate language.");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in AuthenticateUser: {ex.Message}");
            await Clients.Caller.SendAsync("ReceiveSystemMessage", 
                "Error", "An error occurred while processing your request.");
            return false;
        }
    }
}

// Add this class for deserialization
internal class SentimentAnalysisResult
{
    public int SentimentScore { get; set; } 
    public string ToxicityLevel { get; set; } = string.Empty;
    public List<string> Emotions { get; set; } = new List<string>();
    public bool RequiresIntervention { get; set; }
    public string InterventionReason { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
} 