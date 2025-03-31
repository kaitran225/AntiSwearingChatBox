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

    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
} 