using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentsPage : Page
{
    public StudentsViewModel ViewModel { get; } = new();

    public StudentsPage()
    {
        InitializeComponent();
        Loaded += StudentsPage_Loaded;
    }

    private async void StudentsPage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }
}
