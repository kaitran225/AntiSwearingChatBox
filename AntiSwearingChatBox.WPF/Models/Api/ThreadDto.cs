using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.WPF.Models.Api
{
    public class ThreadDto
    {
        public int ThreadId { get; set; }
        public string? Title { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
        public bool ModerationEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int SwearingScore { get; set; }
        public bool IsClosed { get; set; }
        public List<ParticipantDto>? Participants { get; set; }
    }

    public class ParticipantDto
    {
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
    }
} 