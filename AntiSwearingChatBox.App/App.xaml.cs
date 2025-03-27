using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Repositories;
using AntiSwearingChatBox.Service.IServices;
using AntiSwearingChatBox.Service.Services;
using Microsoft.Extensions.Configuration;

namespace Anti_Swearing_Chat_Box.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
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

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserWarningsRepository, UserWarningsRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IThreadsRepository, ThreadsRepository>();
        services.AddScoped<IThreadParticipantsRepository, ThreadParticipantsRepository>();
        services.AddScoped<IMessageHistoryRepository, MessageHistoryRepository>();
        services.AddScoped<IFilteredWordsRepository, FilteredWordsRepository>();

        // Services
        services.AddScoped<IUserWarningsService, UserWarningsService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IThreadsService, ThreadsService>();
        services.AddScoped<IThreadParticipantsService, ThreadParticipantsService>();
        services.AddScoped<IMessageHistoryService, MessageHistoryService>();
        services.AddScoped<IFilteredWordsService, FilteredWordsService>();

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

