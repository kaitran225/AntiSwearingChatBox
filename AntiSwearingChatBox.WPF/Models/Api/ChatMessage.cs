using System;

namespace AntiSwearingChatBox.WPF.Models.Api
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int ThreadId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsFiltered { get; set; }
        public string OriginalContent { get; set; } = string.Empty;
    }
} 