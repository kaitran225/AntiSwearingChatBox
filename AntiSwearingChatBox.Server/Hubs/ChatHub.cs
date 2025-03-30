using AntiSwearingChatBox.AI.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using Microsoft.AspNetCore.SignalR;

namespace AntiSwearingChatBox.Server.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, UserConnection> _userConnections = new();
    private readonly IProfanityFilter _profanityFilter;
    private readonly IMessageHistoryService _messageHistoryService;
    private readonly IChatThreadService _chatThreadService;
    private readonly IUserService _userService;
    private readonly IThreadParticipantService _threadParticipantService;

    public ChatHub(
        IProfanityFilter profanityFilter, 
        IMessageHistoryService messageHistoryService,
        IChatThreadService chatThreadService,
        IUserService userService,
        IThreadParticipantService threadParticipantService)
    {
        _profanityFilter = profanityFilter;
        _messageHistoryService = messageHistoryService;
        _chatThreadService = chatThreadService;
        _userService = userService;
        _threadParticipantService = threadParticipantService;
    }

    public async Task SendMessage(string message, int threadId = 1)
    {
        if (!_userConnections.TryGetValue(Context.ConnectionId, out var connection))
        {
            await Clients.Caller.SendAsync("Error", "You must join the chat before sending messages.");
            return;
        }

        string username = connection.Username;
        int userId = connection.UserId;
        
        // Verify user is a participant in this thread
        var participants = _threadParticipantService.GetByThreadId(threadId);
        if (!participants.Any(p => p.UserId == userId))
        {
            await Clients.Caller.SendAsync("Error", "You are not a participant in this thread.");
            return;
        }
        
        // Filter message for profanity
        var filteredMessage = await _profanityFilter.FilterTextAsync(message);
        bool wasModified = filteredMessage != message;
        
        // Check if message was filtered
        bool containedProfanity = await _profanityFilter.ContainsProfanityAsync(message);
        
        if (containedProfanity)
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
            var thread = _chatThreadService.GetById(threadId);
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
        await Clients.Group($"thread_{threadId}").SendAsync("ReceiveMessage", username, filteredMessage, userId, DateTime.UtcNow);
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
                    msg.CreatedAt
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
                    msg.CreatedAt
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
} 