using System;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows;

namespace AntiSwearingChatBox.WPF.Models.Api
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int ThreadId { get; set; }
        public int UserId { get; set; }
        
        [JsonProperty("OriginalMessage")]
        public string OriginalMessage { get; set; } = string.Empty;
        
        [JsonProperty("ModeratedMessage")]
        public string ModeratedMessage { get; set; } = string.Empty;
        
        // Content property for UI binding, uses ModeratedMessage
        public string Content => ModeratedMessage;
        
        public bool WasModified { get; set; }
        public bool IsFiltered => WasModified;
        
        [JsonProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; }
        
        // Timestamp property for UI binding, uses CreatedAt
        public DateTime Timestamp => CreatedAt;
        
        [JsonProperty("User")]
        public UserModel User { get; set; } = new UserModel();
        
        // SenderName property for UI binding, uses User.Username
        public string SenderName => User?.Username ?? string.Empty;

        // Returns appropriate text color for the bubble based on message state
        public object BubbleForeground => 
            Application.Current.Resources[IsFiltered ? "WarningTextBrush" : "PrimaryTextBrush"];

        public int? ThreadSwearingScore { get; set; }
    }
} 