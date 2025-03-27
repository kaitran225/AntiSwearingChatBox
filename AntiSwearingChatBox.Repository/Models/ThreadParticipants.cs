using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class ThreadParticipants
{
    public int ThreadId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual Threads Thread { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
