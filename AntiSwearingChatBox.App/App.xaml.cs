using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using AntiSwearingChatBox.Service;
using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AntiSwearingChatBox.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Microsoft.Extensions.DependencyInjection.ServiceProvider serviceProvider;
        
        public Microsoft.Extensions.DependencyInjection.ServiceProvider ServiceProvider => serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register database context
            services.AddDbContext<AntiSwearingChatBoxContext>(options =>
            {
                // In a real application, connection string would come from configuration
                string connectionString = GetConnectionString();
                options.UseSqlServer(connectionString);
            });

            // Register repositories
            services.AddScoped<IMessageHistoryRepository, MessageHistoryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register services
            services.AddScoped<IMessageHistoryService, MessageHistoryService>();

            // Register views
            services.AddTransient<Views.ChatWindow>();
            services.AddTransient<Views.LoginWindow>();
        }

        private string GetConnectionString()
        {
            try
            {
                // Find the Service project directory
                string serviceDirectory = FindServiceProjectDirectory();
                
                // Load connection string from Service project's appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(serviceDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();
                
                string? connString = config["ConnectionStrings:AntiSwearingChatBox"];
                
                if (!string.IsNullOrEmpty(connString))
                {
                    return connString;
                }
                
                // Fallback connection string if not found
                return "Server=localhost\\SQLEXPRESS;Database=AntiSwearingChatBox;Trusted_Connection=True;TrustServerCertificate=True;";
            }
            catch
            {
                // Fallback connection string if config file not found
                return "Server=localhost\\SQLEXPRESS;Database=AntiSwearingChatBox;Trusted_Connection=True;TrustServerCertificate=True;";
            }
        }
        
        private string FindServiceProjectDirectory()
        {
            // Start from current directory
            string? currentDir = Directory.GetCurrentDirectory();
            
            // Try to find solution root by traversing up
            while (currentDir != null && !File.Exists(Path.Combine(currentDir, "AntiSwearingChatBox.sln")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }
            
            // If found solution root, look for Service project
            if (currentDir != null)
            {
                string serviceDir = Path.Combine(currentDir, "AntiSwearingChatBox.Service");
                if (Directory.Exists(serviceDir))
                {
                    return serviceDir;
                }
            }
            
            // Fallback to current directory
            return Directory.GetCurrentDirectory();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get the login window from the service provider
            var loginWindow = serviceProvider.GetService<Views.LoginWindow>();
            loginWindow?.Show();
        }
    }
}
