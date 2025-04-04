namespace AntiSwearingChatBox.WPF.Services.Api
{
    public static class ApiConfig
    {
        // Base URL for the API
        public static string BaseUrl { get; set; } = "http://localhost:5000";
       
        // API Endpoints
        public static string LoginEndpoint => $"{BaseUrl}/api/auth/login";
        public static string RegisterEndpoint => $"{BaseUrl}/api/auth/register";
        public static string ThreadsEndpoint => $"{BaseUrl}/api/chat/threads";
        public static string MessagesEndpoint => $"{BaseUrl}/api/chat/messages";
        
        // SignalR Hub URL - Correct path based on server implementation
        public static string ChatHubUrl => $"{BaseUrl}/chatHub";
    }
} 