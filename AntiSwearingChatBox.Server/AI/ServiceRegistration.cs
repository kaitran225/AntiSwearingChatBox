using AntiSwearingChatBox.Server.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AntiSwearingChatBox.AI
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddGeminiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GeminiSettings>(options => 
            {
                options.ApiKey = configuration["GeminiSettings:ApiKey"] ?? "AIzaSyAr-Vto1YywEwssTDzeEmkS2P4caVaU13o";
                options.ModelName = configuration["GeminiSettings:ModelName"] ?? "gemini-2.0-flash-lite";
            });
            
            services.AddSingleton<GeminiService>();
            services.AddTransient<GeminiController>();
            
            return services;
        }
    }
} 