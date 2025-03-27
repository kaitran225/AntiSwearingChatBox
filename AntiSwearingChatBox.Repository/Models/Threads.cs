using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class Threads
{
    public int ThreadId { get; set; }

    public string Title { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime LastMessageAt { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<MessageHistory> MessageHistory { get; set; } = new List<MessageHistory>();

    public virtual ICollection<ThreadParticipants> ThreadParticipants { get; set; } = new List<ThreadParticipants>();

    public virtual ICollection<UserWarnings> UserWarnings { get; set; } = new List<UserWarnings>();
}
