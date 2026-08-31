using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentLoginPage : Page
{
    public StudentLoginViewModel ViewModel { get; }

    public StudentLoginPage()
    {
        InitializeComponent();
        ViewModel = new StudentLoginViewModel(((App)Application.Current).Services.GetRequiredService<AuthenticationService>());
        ViewModel.LoginSucceeded += LoginSucceeded;
        DataContext = ViewModel;
    }

    private void Password_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox) ViewModel.Password = passwordBox.Password;
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame?.GoBack();

    private void LoginSucceeded(object? sender, EventArgs e)
    {
        SessionService.StartStudent(ViewModel.StudentId.Trim());
        Frame?.Navigate(typeof(StudentShellPage));
    }
}
