using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentPortalPage : Page
{
    public StudentPortalViewModel ViewModel { get; } = new();

    public StudentPortalPage()
    {
        InitializeComponent();
    }

    public async Task LoadStudentAsync(string studentId) => await ViewModel.LoadAsync(studentId);

    private void SignOut_Click(object sender, RoutedEventArgs e)
        => Frame?.Navigate(typeof(LoginPage));
}
