using Newtonsoft.Json;

namespace AntiSwearingChatBox.WPF.Models.Api
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        [JsonProperty("User")]
        public UserModel? User { get; set; }

        public string Username => User?.Username ?? string.Empty;
        public int UserId => User?.UserId ?? 0;
    }
} 