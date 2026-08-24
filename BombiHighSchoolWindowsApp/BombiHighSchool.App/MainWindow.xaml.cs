using Microsoft.UI.Xaml;
using BombiHighSchool.App.Views;

namespace BombiHighSchool.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RootFrame.Navigate(typeof(LoginPage));
    }
}
