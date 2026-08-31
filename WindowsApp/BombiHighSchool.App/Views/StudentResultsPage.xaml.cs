using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentResultsPage : Page
{
    public StudentResultsViewModel ViewModel { get; } = new();
    public StudentResultsPage() => InitializeComponent();
    public async Task LoadStudentAsync(string studentId) => await ViewModel.LoadAsync(studentId);
}
