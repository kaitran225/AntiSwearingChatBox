using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anti_Swearing_Chat_Box.AI
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddGeminiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GeminiSettings>(options => 
            {
                options.ApiKey = configuration["GeminiSettings:ApiKey"] ?? "AIzaSyD9Odq-PkFqA2HHYsR86EEhPbM85eHF2Sw";
                options.ModelName = configuration["GeminiSettings:ModelName"] ?? "gemini-pro";
            });
            
            services.AddSingleton<GeminiService>();
            services.AddTransient<GeminiController>();
            
            return services;
        }
    }
} 