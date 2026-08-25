using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register Services
        services.AddSingleton<ILocalDataService, LocalDataService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<AuthenticationService>();
        
        // Register ViewModels
        services.AddTransient<AdminAcademicViewModel>();
    }
}
