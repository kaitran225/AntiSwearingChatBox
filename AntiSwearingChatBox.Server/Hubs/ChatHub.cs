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
                var thread = _chatThreadService.GetById(threadId);
                if (thread != null)
                {
                    thread.SwearingScore = newScore;
                    _chatThreadService.Update(thread);
                    
                    await Clients.Group($"thread_{threadId}").SendAsync(
                        "ThreadInfoUpdated", 
                        threadId,
                        thread.Title,
                        thread.IsPrivate,
                        thread.SwearingScore,
                        thread.IsClosed
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating swearing score: {ex.Message}");
            }
        }
        
        private class UserConnection
        {
            public string Username { get; set; } = string.Empty;
            public int UserId { get; set; }
        }
    }
} 