using Microsoft.UI.Xaml;

namespace BombiHighSchool.App;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }

    public App() { InitializeComponent(); }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
