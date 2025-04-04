using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.WPF.Models
{
    public class Message
    {
        public string? Id { get; set; }
        public string? ThreadId { get; set; }
        public string? FromUserId { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public bool IsSent { get; set; }
    }
} 