using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        var dialog = new ContentDialog
        {
            Title = "Archive student?",
            Content = $"{student.Name} will be preserved in the records but marked archived and their account will be disabled. Nothing is permanently deleted.",
            PrimaryButtonText = "Archive student",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            await ViewModel.ArchiveCommand.ExecuteAsync(student);
    }
}
