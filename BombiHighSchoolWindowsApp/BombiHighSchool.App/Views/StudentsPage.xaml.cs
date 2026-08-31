using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentsPage : Page
{
    public StudentsViewModel ViewModel { get; } = new();
    public StudentsPage() { InitializeComponent(); Loaded += StudentsPage_Loaded; }
    private async void StudentsPage_Loaded(object sender, RoutedEventArgs e) => await ViewModel.LoadCommand.ExecuteAsync(null);
    public void FocusSearch(string query) => ViewModel.SearchText = query;
    public void StartNewStudent() => ViewModel.CancelEditCommand.Execute(null);
    private void NewStudent_Click(object sender, RoutedEventArgs e) => StartNewStudent();

    private async void ArchiveSelected_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedStudent is null) return;
        var student = ViewModel.SelectedStudent;
        if (await AppUxServices.ConfirmAsync(XamlRoot,
            "Archive student?",
            $"{student.Name} will remain in the records but be marked archived and their account disabled. Nothing is permanently deleted.",
            "Archive student") )
        {
            await ViewModel.ArchiveCommand.ExecuteAsync(student);
        }
    }
}
