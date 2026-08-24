using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new();

    public SettingsPage() => InitializeComponent();

    private void CurrentPassword_Changed(object sender, RoutedEventArgs e) { if (sender is PasswordBox box) ViewModel.CurrentPassword = box.Password; }
    private void NewPassword_Changed(object sender, RoutedEventArgs e) { if (sender is PasswordBox box) ViewModel.NewPassword = box.Password; }
    private void ConfirmPassword_Changed(object sender, RoutedEventArgs e) { if (sender is PasswordBox box) ViewModel.ConfirmPassword = box.Password; }
}
