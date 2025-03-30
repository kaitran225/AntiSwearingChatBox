using AntiSwearingChatBox.Repository.Models;

namespace AntiSwearingChatBox.Server.Repo.Models;

public partial class UserWarning
{
    public int WarningId { get; set; }

    public int UserId { get; set; }

    public int ThreadId { get; set; }

    public string WarningMessage { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ChatThread Thread { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
