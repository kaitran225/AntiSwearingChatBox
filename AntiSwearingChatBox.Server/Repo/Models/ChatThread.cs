using AntiSwearingChatBox.Server.Repo.Models;
using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class ChatThread
{
    public int ThreadId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime LastMessageAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsPrivate { get; set; }

    public bool AllowAnonymous { get; set; }

    public bool ModerationEnabled { get; set; }

    public int? MaxParticipants { get; set; }

    public int? AutoDeleteAfterDays { get; set; }
    
    public int SwearingScore { get; set; } = 0;
    
    public bool IsClosed { get; set; } = false;

    public virtual ICollection<MessageHistory> MessageHistories { get; set; } = new List<MessageHistory>();

    public virtual ICollection<ThreadParticipant> ThreadParticipants { get; set; } = new List<ThreadParticipant>();

    public virtual ICollection<UserWarning> UserWarnings { get; set; } = new List<UserWarning>();
}
