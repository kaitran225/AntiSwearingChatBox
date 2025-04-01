using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AntiSwearingChatBox.WPF.Services;

namespace AntiSwearingChatBox.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // Register services
        services.AddSingleton<ApiService>();
    }
}

