using System;
using AntiSwearingChatBox.App.Services;

namespace AntiSwearingChatBox.App.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }
        public string ModeratedText { get; set; }
        public bool WasModified { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApiService.User User { get; set; }
        public string Username { get; set; }
        public bool IsFromCurrentUser { get; set; }
        public string Avatar { get; set; }
        public string Background { get; set; }
        public string BorderBrush { get; set; }
        public string Timestamp => CreatedAt.ToString("h:mm tt");
    }
} 