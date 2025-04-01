using AntiSwearingChatBox.WPF.Services.Api;

namespace AntiSwearingChatBox.WPF.Services
{
    public static class ServiceProvider
    {
        private static ApiService? _apiService;
        
        public static ApiService ApiService => _apiService ??= new ApiService();
    }
} 