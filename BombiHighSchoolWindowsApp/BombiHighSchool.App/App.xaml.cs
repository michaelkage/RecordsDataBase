using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    // Block 2: The Service Provider engine
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
        // Register the window directly into the DI container!
        services.AddTransient<MainWindow>(); 
        services.AddTransient<AdminAcademicViewModel>();
    }

    // Block 1: Use the engine inside the lifecycle launch
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Instead of 'new MainWindow()', ask the DI engine to build it 
        // and automatically inject any required services into its constructor.
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Activate();
    }
}
