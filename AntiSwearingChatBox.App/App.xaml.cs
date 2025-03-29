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
                // Try to get connection string from app settings
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                return config["ConnectionStrings:AntiSwearingChatBox"] 
                    ?? "Server=(localdb)\\MSSQLLocalDB;Database=AntiSwearingChatBox;Trusted_Connection=True;";
            }
            catch
            {
                // Fallback connection string if config file not found
                return "Server=(localdb)\\MSSQLLocalDB;Database=AntiSwearingChatBox;Trusted_Connection=True;";
            }
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
