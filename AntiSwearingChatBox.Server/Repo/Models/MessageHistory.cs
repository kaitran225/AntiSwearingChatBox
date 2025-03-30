using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class MessageHistory
{
    public int MessageId { get; set; }

    public int ThreadId { get; set; }

    public int UserId { get; set; }

    public string OriginalMessage { get; set; } = null!;

    public string? ModeratedMessage { get; set; }

    public bool WasModified { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ChatThread Thread { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
