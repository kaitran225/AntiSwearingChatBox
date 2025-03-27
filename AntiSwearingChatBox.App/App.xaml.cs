using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Repositories;
using AntiSwearingChatBox.Service.IServices;
using AntiSwearingChatBox.Service.Services;
using Microsoft.Extensions.Configuration;

namespace AntiSwearingChatBox.App;

public partial class App
{
    private ServiceProvider? _serviceProvider;
    
    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // DbContext
        services.AddDbContext<AntiSwearingChatBoxContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Main Window
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider?.GetService<MainWindow>();
        if (mainWindow != null)
        {
            mainWindow.Show();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

