using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.ChatServer
{
    public class ChatServer
    {
        private readonly int _port;
        private WebApplication _app;

        public ChatServer(int port)
        {
            _port = port;
        }

        public async Task StartAsync()
        {
            try
            {
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls($"http://0.0.0.0:{_port}");
                
                // Add SignalR services
                builder.Services.AddSignalR();
                
                // Register profanity filter service
                builder.Services.AddSingleton<IProfanityFilter, SimpleProfanityFilter>();
                
                // Configure CORS for local network
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", builder =>
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader());
                });
                
                _app = builder.Build();
                
                // Configure the HTTP request pipeline
                _app.UseCors("CorsPolicy");
                
                // Map the ChatHub
                _app.MapHub<ChatHub>("/chatHub");
                
                Console.WriteLine($"Starting server on port {_port}...");
                await _app.RunAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Server error: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
            }
        }
    }
} 