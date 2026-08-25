using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.ViewModels; // Ensure your ViewModels namespace is imported
using BombiHighSchool.App.Views;        // Ensure your Views namespace is imported

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
        // 1. Infrastructure and Core Business Logic Services
        services.AddSingleton<ILocalDataService, LocalDataService>();
        // Add other cross-cutting services if you have them, e.g.:
        // services.AddScoped<IReportingService, ReportingService>();
        // services.AddScoped<AuthenticationService>();

        // 2. ViewModels (Kept transient so they clear memory when closed)
        services.AddTransient<AdminAcademicViewModel>();
        // Add other viewmodels here as you build them out, e.g.:
        // services.AddTransient<StudentsViewModel>();

        // 3. UI Shell Windows and Host Pages
        services.AddTransient<MainWindow>(); 
        services.AddTransient<MainPage>();

        // 4. Sub-Navigation Content Pages
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
        // Resolves MainWindow, which automatically injects its contents via the container
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Activate();
    }
}
