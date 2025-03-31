using System;
using System.Collections.Generic;
using AntiSwearingChatBox.App.Services;

namespace AntiSwearingChatBox.App.Models
{
    public class ChatThread
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPrivate { get; set; }
        public int CreatorUserId { get; set; }
        public List<ApiService.Participant> Participants { get; set; } = new List<ApiService.Participant>();
        public Message LastMessage { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
} 