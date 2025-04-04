using AntiSwearingChatBox.WPF.Services.Api;

namespace AntiSwearingChatBox.WPF.Services
{
    public static class ServiceProvider
    {
        private static IApiService? _apiService;

        public static IApiService ApiService 
        {
            get 
            {
                if (_apiService == null)
                {
                    _apiService = new ApiService();
                }
                return _apiService;
            }
        }
    }
} 