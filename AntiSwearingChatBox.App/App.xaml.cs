using System.Windows;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AntiSwearingChatBox.App.Services;

namespace AntiSwearingChatBox.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Microsoft.Extensions.DependencyInjection.ServiceProvider serviceProvider;
        private IConfiguration configuration;
        
        public Microsoft.Extensions.DependencyInjection.ServiceProvider ServiceProvider => serviceProvider;

        public App()
        {
            // Load configuration from appsettings.json
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            configuration = configBuilder.Build();
                
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);
            
            // Register services
            services.AddSingleton<ApiService>(); 

            // Register views (windows and pages)
            services.AddTransient<Views.MainWindow>();
            services.AddTransient<Views.LoginPage>();
            services.AddTransient<Views.RegisterPage>();
            services.AddTransient<Views.ChatPage>();
            services.AddTransient<Views.UserSelectionPage>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Get the main window
            var mainWindow = serviceProvider.GetService<Views.MainWindow>();
            mainWindow?.Show();
        }
    }
}
