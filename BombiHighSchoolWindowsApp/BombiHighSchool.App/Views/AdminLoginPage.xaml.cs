using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class AdminLoginPage : Page
{
    public AdminLoginViewModel ViewModel { get; } = new();

    public AdminLoginPage()
    {
        InitializeComponent();
        ViewModel.LoginSucceeded += LoginSucceeded;
    }

    private void Password_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
            ViewModel.Password = passwordBox.Password;
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame?.GoBack();

    private void LoginSucceeded(object? sender, EventArgs e)
        => Frame?.Navigate(typeof(MainPage));
}
