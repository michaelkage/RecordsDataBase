using Microsoft.UI.Xaml;
using BombiHighSchool.App.Views;

namespace BombiHighSchool.App;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Current { get; private set; }

    public MainWindow()
    {
        Current = this;
        InitializeComponent();
        RootFrame.Navigate(typeof(LoginPage));
        Closed += (_, _) => Services.SessionService.Clear();
    }
}
