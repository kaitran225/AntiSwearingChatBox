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
        
        // Add deconstructor to allow tuple deconstruction
        public void Deconstruct(out bool success, out string message)
        {
            success = Success;
            message = Message;
        }
        
        // Common tuple pattern used in CLI
        public static implicit operator (bool Success, string Message)(AuthResponse response)
        {
            return (response.Success, response.Message);
        }
        
        public static implicit operator (bool Success, string Message, UserModel? User)(AuthResponse response)
        {
            return (response.Success, response.Message, response.User);
        }
    }
} 