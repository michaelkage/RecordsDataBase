using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentPortalPage : Page
{
    public StudentPortalPage()
    {
        InitializeComponent();
    }

    private void SignOut_Click(object sender, RoutedEventArgs e)
        => Frame?.Navigate(typeof(LoginPage));
}
