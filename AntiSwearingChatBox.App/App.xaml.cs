using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
            // Simplified initialization
            var configBuilder = new ConfigurationBuilder();
            configuration = configBuilder.Build();
                
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Only register views (windows and pages)
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
