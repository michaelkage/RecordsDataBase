using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentSubjectsPage : Page
{
    public StudentSubjectsViewModel ViewModel { get; } = new();
    public StudentSubjectsPage() => InitializeComponent();
    public async Task LoadStudentAsync(string studentId) => await ViewModel.LoadAsync(studentId);
}
