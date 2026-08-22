using Microsoft.UI.Xaml;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.Models;

namespace BombiHighSchool.App;

public partial class App : Application
{
    public static SchoolData SchoolData { get; private set; } = new();

    public static LocalDataService DataService { get; } = new();

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        SchoolData = await DataService.LoadAsync();

        var window = new MainWindow();
        window.Activate();
    }
}