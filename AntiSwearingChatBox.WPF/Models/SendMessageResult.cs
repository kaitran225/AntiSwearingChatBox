using System;
using AntiSwearingChatBox.WPF.Models.Api;

namespace AntiSwearingChatBox.WPF.Models
{
    public class SendMessageResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ChatMessage MessageHistory { get; set; } = new ChatMessage();
        public bool WasModerated { get; set; }
    }
} 