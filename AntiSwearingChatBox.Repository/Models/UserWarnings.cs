using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class UserWarnings
{
    public int WarningId { get; set; }

    public int UserId { get; set; }

    public int ThreadId { get; set; }

    public string WarningMessage { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Threads Thread { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
