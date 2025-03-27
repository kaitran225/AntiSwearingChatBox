using System;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Repository.Models;

public partial class FilteredWord
{
    public int WordId { get; set; }

    public string Word { get; set; } = null!;

    public int SeverityLevel { get; set; }

    public DateTime CreatedAt { get; set; }
}
