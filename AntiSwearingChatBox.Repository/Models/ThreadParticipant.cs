using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class ThreadParticipant
{
    public int ThreadId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual ChatThread Thread { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
