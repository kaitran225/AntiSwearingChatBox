using AntiSwearingChatBox.AI.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AntiSwearingChatBox.Server.Hubs;

public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> _connectedUsers = new();
    private readonly IProfanityFilter _profanityFilter;

    public ChatHub(IProfanityFilter profanityFilter)
    {
        _profanityFilter = profanityFilter;
    }

    public async Task SendMessage(string message)
    {
        string user = _connectedUsers[Context.ConnectionId];
        
        // Filter message for profanity
        var filteredMessage = await _profanityFilter.FilterTextAsync(message);
        
        // Check if message was filtered
        bool containedProfanity = await _profanityFilter.ContainsProfanityAsync(message);
        
        if (containedProfanity)
        {
            // Send a private warning to the user
            await Clients.Caller.SendAsync("PrivateMessage", "System", "Your message contained inappropriate language and was filtered.");
        }
        
        // Broadcast filtered message to all clients
        await Clients.All.SendAsync("ReceiveMessage", user, filteredMessage);
    }

    public async Task JoinChat(string username)
    {
        // Check if username contains profanity
        if (await _profanityFilter.ContainsProfanityAsync(username))
        {
            await Clients.Caller.SendAsync("Error", "Username contains inappropriate language. Please choose another username.");
            return;
        }

        _connectedUsers[Context.ConnectionId] = username;
        await Clients.Others.SendAsync("UserJoined", username);
        await Clients.Caller.SendAsync("JoinConfirmed", username);
        
        // Send the list of connected users to the new client
        await Clients.Caller.SendAsync("UserList", _connectedUsers.Values.ToList());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out string? username))
        {
            _connectedUsers.Remove(Context.ConnectionId);
            await Clients.Others.SendAsync("UserLeft", username);
        }

        await base.OnDisconnectedAsync(exception);
    }
} 