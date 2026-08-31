using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class AdminLoginPage : Page
{
    public AdminLoginViewModel ViewModel { get; }

    public AdminLoginPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<AdminLoginViewModel>();
        ViewModel.LoginSucceeded += LoginSucceeded;
        DataContext = ViewModel;
    }

    private void Password_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
            ViewModel.Password = passwordBox.Password;
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame?.GoBack();

    private void LoginSucceeded(object? sender, EventArgs e)
    {
        // MainPage resolves its own services from DI, so navigation can safely use
        // WinUI's normal parameterless page activation here.
        Frame?.Navigate(typeof(MainPage));
    }
}
