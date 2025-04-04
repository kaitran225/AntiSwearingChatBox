using System;

namespace AntiSwearingChatBox.WPF.Models.Api
{
    public class ChatThread
    {
        public int ThreadId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int? SwearingScore { get; set; }
        public bool IsClosed { get; set; }
    }
} 