using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AntiSwearingChatBox.AI.Filter;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using AntiSwearingChatBox.Server.AI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AntiSwearingChatBox.Server.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, UserConnection> _userConnections = new Dictionary<string, UserConnection>();
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
        
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
        
        public async Task UpdateThreadSwearingScore(int threadId, int newScore)
        {
            try
            {
                _logger.LogInformation($"Updating thread {threadId} swearing score to {newScore}");
                
                // Get the current thread
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    _logger.LogWarning($"Thread {threadId} not found for swearing score update");
                    return;
                }
                
                // Check if there's an actual change
                bool scoreChanged = thread.SwearingScore != newScore;
                
                // Update the score in the database
                thread.SwearingScore = newScore;
                var result = _chatThreadService.Update(thread);
                
                if (!result.success)
                {
                    _logger.LogError($"Failed to update thread {threadId} swearing score: {result.message}");
                    return;
                }
                
                // Send the updated score to all clients in the thread
                await Clients.Group($"thread_{threadId}").SendAsync(
                    "SwearingScoreUpdated", 
                    threadId,
                    newScore
                );
                
                // Check if we need to take action based on score
                if (newScore >= 5)
                {
                    // Close the thread due to excessive profanity
                    await CloseThread(threadId, "Thread closed due to excessive profanity");
                }
                else if (scoreChanged)
                {
                    // If score changed, update the chat view
                    await UpdateChatView(threadId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating swearing score: {ex.Message}");
            }
        }
        
        public async Task UpdateChatView(int threadId)
        {
            try
            {
                // Get thread details
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    _logger.LogWarning($"Thread {threadId} not found for chat view update");
                    return;
                }

                // Get the latest messages (e.g., last 20)
                var messages = _messageHistoryService.GetByThreadId(threadId).OrderByDescending(m => m.CreatedAt).Take(20).ToList();
                
                // Send thread info update to all clients in the thread
                await Clients.Group($"thread_{threadId}").SendAsync(
                    "ThreadInfoUpdated",
                    thread.ThreadId,
                    thread.IsClosed,
                    thread.SwearingScore,
                    thread.LastMessageAt
                );
                
                _logger.LogInformation($"Updated thread {threadId} view - SwearingScore: {thread.SwearingScore}, Messages: {messages?.Count ?? 0}");
                
                // If there are messages, send the latest one to ensure all clients see it
                if (messages != null && messages.Count > 0)
                {
                    var latestMessage = messages[0]; // Assuming newest first
                    
                    // Get the user who sent the message
                    var user = _userService.GetById(latestMessage.UserId);
                    string username = user?.Username ?? "Unknown User";
                    
                    // Broadcast the message to ensure all clients have the latest
                    await Clients.Group($"thread_{threadId}").SendAsync(
                        "ReceiveMessage",
                        username,
                        latestMessage.ModeratedMessage,
                        latestMessage.UserId,
                        latestMessage.CreatedAt,
                        threadId,
                        latestMessage.OriginalMessage,
                        latestMessage.WasModified,
                        thread.SwearingScore
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating chat view: {ex.Message}");
            }
        }
        
        public async Task CloseThread(int threadId, string reason = "Thread closed by moderator")
        {
            try
            {
                // Get the thread
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    _logger.LogWarning($"Thread {threadId} not found for closure");
                    return;
                }
                
                // Check if already closed
                if (thread.IsClosed)
                {
                    _logger.LogInformation($"Thread {threadId} is already closed");
                    return;
                }
                
                // Update thread status
                thread.IsClosed = true;
                thread.LastMessageAt = DateTime.UtcNow; // Use LastMessageAt instead of ClosedAt
                var result = _chatThreadService.Update(thread);
                
                if (!result.success)
                {
                    _logger.LogError($"Failed to close thread {threadId}: {result.message}");
                    return;
                }
                
                _logger.LogInformation($"Thread {threadId} closed. Reason: {reason}");
                
                // Notify all clients in the thread
                await Clients.Group($"thread_{threadId}").SendAsync("ThreadClosed", threadId, reason);
                
                // Add a system message about the closure
                var systemMessage = new MessageHistory
                {
                    ThreadId = threadId,
                    UserId = 0, // System user
                    OriginalMessage = reason,
                    ModeratedMessage = reason,
                    WasModified = false,
                    CreatedAt = DateTime.UtcNow
                };
                
                _messageHistoryService.Add(systemMessage);
                
                // Send updated thread info
                await Clients.Group($"thread_{threadId}").SendAsync(
                    "ThreadInfoUpdated",
                    thread.ThreadId,
                    thread.IsClosed,
                    thread.SwearingScore,
                    thread.LastMessageAt
                );
                
                // Trigger a full chat view update to ensure synchronization
                await UpdateChatView(threadId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error closing thread: {ex.Message}");
            }
        }
        
        public async Task SendMessage(int threadId, string message, int userId, string username)
        {
            try
            {
                // Get the thread
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    await Clients.Caller.SendAsync("Error", "Thread not found");
                    return;
                }
                
                // Check if thread is closed
                if (thread.IsClosed)
                {
                    await Clients.Caller.SendAsync("Error", "This thread is closed");
                    return;
                }
                
                // Process the message with profanity filter
                string filteredMessage = await _profanityFilter.FilterProfanityAsync(message);
                bool wasModified = filteredMessage != message;
                int profanityScore = wasModified ? 1 : 0; // Simple score calculation
                
                _logger.LogInformation($"Message from {username} - Modified: {wasModified}, " +
                    $"Score Change: {profanityScore}, Thread: {threadId}");
                
                // Store the message
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
                    await Clients.Caller.SendAsync("Error", "Failed to save message");
                    return;
                }
                
                // Update timestamp
                thread.LastMessageAt = DateTime.UtcNow;
                
                // If profanity was detected, update the thread's swearing score
                if (wasModified && profanityScore > 0)
                {
                    // Increment swearing score based on the detected profanity
                    thread.SwearingScore += profanityScore;
                    _logger.LogWarning($"Thread {threadId} swearing score increased to {thread.SwearingScore}");
                }
                
                _chatThreadService.Update(thread);
                
                // Broadcast to all users in the thread
                await Clients.Group($"thread_{threadId}").SendAsync(
                    "ReceiveMessage",
                    username,
                    filteredMessage,
                    userId,
                    DateTime.UtcNow,
                    threadId,
                    message,
                    wasModified,
                    thread.SwearingScore
                );
                
                // If the message was modified, update the swearing score for all clients
                if (wasModified)
                {
                    await UpdateThreadSwearingScore(threadId, thread.SwearingScore);
                }
                else
                {
                    // Trigger a chat view update to ensure all clients are synchronized
                    await UpdateChatView(threadId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "An error occurred");
            }
        }
        
        public async Task JoinThread(int threadId)
        {
            try
            {
                var thread = _chatThreadService.GetById(threadId);
                if (thread == null)
                {
                    await Clients.Caller.SendAsync("Error", "Thread not found");
                    return;
                }
                
                // Add to the thread's group for SignalR updates
                await Groups.AddToGroupAsync(Context.ConnectionId, $"thread_{threadId}");
                
                // Store connection info
                var connectionId = Context.ConnectionId;
                var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
                
                // Safely log user info
                string userIdForLogging = !string.IsNullOrEmpty(userId) ? userId : "unknown";
                _logger.LogInformation($"User {userIdForLogging} joined thread {threadId}");
                
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
                {
                    _userConnections[connectionId] = new UserConnection
                    {
                        ThreadId = threadId,
                        UserId = userIdInt
                    };
                }
                
                // Send thread info to the client
                await Clients.Caller.SendAsync("ThreadInfoUpdated", 
                    thread.ThreadId, 
                    thread.IsClosed, 
                    thread.SwearingScore, 
                    thread.LastMessageAt);
                
                // Update the chat view for this user to get latest messages
                await UpdateChatView(threadId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error joining thread: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "Failed to join thread");
            }
        }
        
        public async Task LeaveThread(int threadId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"thread_{threadId}");
                
                // Remove from our connection tracking
                var connectionId = Context.ConnectionId;
                if (_userConnections.ContainsKey(connectionId))
                {
                    _userConnections.Remove(connectionId);
                }
                
                _logger.LogInformation($"Connection {Context.ConnectionId} left thread {threadId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error leaving thread: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "Failed to leave thread");
            }
        }
        
        // When a connection is terminated
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            if (_userConnections.ContainsKey(connectionId))
            {
                var threadId = _userConnections[connectionId].ThreadId;
                if (threadId > 0)
                {
                    await Groups.RemoveFromGroupAsync(connectionId, $"thread_{threadId}");
                }
                _userConnections.Remove(connectionId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        
        private class UserConnection
        {
            public string Username { get; set; } = string.Empty;
            public int UserId { get; set; }
            public int ThreadId { get; set; }
        }
    }
} 