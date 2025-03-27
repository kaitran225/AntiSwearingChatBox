using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class UserWarning
{
    public int WarningId { get; set; }

    public int UserId { get; set; }

    public int ThreadId { get; set; }

    public string WarningMessage { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Thread Thread { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
