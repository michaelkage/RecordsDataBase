using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BombiHighSchool.App.Views;

public sealed partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void Student_Click(object sender, RoutedEventArgs e)
        => Frame?.Navigate(typeof(StudentLoginPage));

    private void Admin_Click(object sender, RoutedEventArgs e)
        => Frame?.Navigate(typeof(AdminLoginPage));
}
