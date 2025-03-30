using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;
using AntiSwearingChatBox.Service.Interface;
using AntiSwearingChatBox.Repository;
using AntiSwearingChatBox.Repository.Interfaces;

namespace AntiSwearingChatBox.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Microsoft.Extensions.DependencyInjection.ServiceProvider serviceProvider;
        private IConfiguration configuration;
        
        // Add property to store current user
        public User? CurrentUser { get; set; }
        
        public Microsoft.Extensions.DependencyInjection.ServiceProvider ServiceProvider => serviceProvider;

        public App()
        {
            // Create configuration
            string serviceDirectory = FindServiceProjectDirectory();
            string appDirectory = Directory.GetCurrentDirectory();
            
            // Build configuration with both possible locations for appsettings.json
            var configBuilder = new ConfigurationBuilder();
            
            // Try app directory first
            string appSettingsAppPath = Path.Combine(appDirectory, "appsettings.json");
            if (File.Exists(appSettingsAppPath))
            {
                configBuilder.SetBasePath(appDirectory);
                configBuilder.AddJsonFile("appsettings.json", optional: false);
            }
            // Then try service directory
            else
            {
                string appSettingsServicePath = Path.Combine(serviceDirectory, "appsettings.json");
                if (File.Exists(appSettingsServicePath))
                {
                    configBuilder.SetBasePath(serviceDirectory);
                    configBuilder.AddJsonFile("appsettings.json", optional: false);
                }
                else
                {
                    // Show error message if appsettings.json is not found in either location
                    MessageBox.Show("appsettings.json not found in either App or Service directories. Authentication services may not work correctly.", 
                                   "Configuration Error", 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Warning);
                    
                    // Create a minimal configuration with JWT settings
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"ConnectionStrings:AntiSwearingChatBox", "Server=localhost\\SQLEXPRESS;Database=AntiSwearingChatBox;Trusted_Connection=True;TrustServerCertificate=True;"},
                        {"JwtSettings:SecretKey", "your-super-secret-key-with-minimum-32-characters"},
                        {"JwtSettings:Issuer", "AntiSwearingChatBox"},
                        {"JwtSettings:Audience", "AntiSwearingChatBox"},
                        {"JwtSettings:ExpirationInMinutes", "60"},
                        {"JwtSettings:RefreshTokenExpirationInDays", "7"}
                    });
                }
            }
            
            configuration = configBuilder.Build();
                
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Register database context
            services.AddDbContext<AntiSwearingChatBoxContext>(options =>
            {
                // In a real application, connection string would come from configuration
                string connectionString = GetConnectionString();
                options.UseSqlServer(connectionString);
            });

            // Register repositories
            services.AddScoped<IMessageHistoryRepository, MessageHistoryRepository>();
            services.AddScoped<IChatThreadRepository, ChatThreadRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IThreadParticipantRepository, ThreadParticipantRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register services
            services.AddScoped<IMessageHistoryService, MessageHistoryService>();
            services.AddScoped<IChatThreadService, ChatThreadService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IThreadParticipantService, ThreadParticipantService>();
            services.AddScoped<IAuthService, AuthService>();

            // Register views (windows and pages)
            services.AddTransient<Views.MainWindow>();
            services.AddTransient<Views.LoginPage>();
            services.AddTransient<Views.RegisterPage>();
            services.AddTransient<Views.ChatPage>();
            services.AddTransient<Views.UserSelectionPage>();
        }

        private string GetConnectionString()
        {
            try
            {
                string? connString = configuration["ConnectionStrings:AntiSwearingChatBox"];
                
                if (!string.IsNullOrEmpty(connString))
                {
                    return connString;
                }
                
                // Fallback connection string if not found
                return "Server=localhost\\SQLEXPRESS;Database=AntiSwearingChatBox;Trusted_Connection=True;TrustServerCertificate=True;";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading connection string: {ex.Message}", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback connection string if config file not found
                return "Server=localhost\\SQLEXPRESS;Database=AntiSwearingChatBox;Trusted_Connection=True;TrustServerCertificate=True;";
            }
        }
        
        private string FindServiceProjectDirectory()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding service directory: {ex.Message}", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Directory.GetCurrentDirectory();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get the main window instead of login window
            var mainWindow = serviceProvider.GetService<Views.MainWindow>();
            mainWindow?.Show();
        }
    }
}
