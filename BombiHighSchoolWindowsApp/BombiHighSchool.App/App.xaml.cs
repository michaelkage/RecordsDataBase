using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.ViewModels;
using BombiHighSchool.App.Views;

namespace BombiHighSchool.App;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App()
    {
        InitializeComponent();
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILocalDataService, LocalDataService>();
        services.AddSingleton<GlobalSearchService>();
        services.AddSingleton<NotificationService>(_ => NotificationService.Instance);

        services.AddTransient<AdminAcademicViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<MainPage>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<StudentsPage>();
        services.AddTransient<SubjectsPage>();
        services.AddTransient<AdminAcademicPage>();
        services.AddTransient<ScoresPage>();
        services.AddTransient<RankingsPage>();
        services.AddTransient<SettingsPage>();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Activate();
    }
}
