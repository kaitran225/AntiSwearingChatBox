using AntiSwearingChatBox.WPF.Services.Api;

namespace AntiSwearingChatBox.WPF.Services
{
    public static class ServiceProvider
    {
        private static IApiService? _apiService;
        
        public static IApiService ApiService => _apiService ??= new ApiService();
    }
} 