using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.Services;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentTranscriptPage : Page
{
    public StudentTranscriptViewModel ViewModel { get; } = new();
    private readonly TranscriptPrintService _printer = new();

    public StudentTranscriptPage()
    {
        InitializeComponent();
        Loaded += (_, _) => _printer.Register(TranscriptSurface);
        Unloaded += (_, _) => _printer.Unregister();
    }

    public async Task LoadStudentAsync(string studentId) => await ViewModel.LoadAsync(studentId);

    private async void Print_Click(object sender, RoutedEventArgs e)
    {
        if (!await _printer.ShowPrintUIAsync())
            ViewModel.StatusMessage = "Windows printing is not available on this device right now.";
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SessionService.StudentId))
            await ViewModel.LoadAsync(SessionService.StudentId);
    }
}
