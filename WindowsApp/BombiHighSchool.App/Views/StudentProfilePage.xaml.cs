using Microsoft.UI.Xaml.Controls;
using BombiHighSchool.App.ViewModels;

namespace BombiHighSchool.App.Views;

public sealed partial class StudentProfilePage : Page
{
    public StudentProfileViewModel ViewModel { get; } = new();
    public StudentProfilePage() => InitializeComponent();

    public async Task LoadStudentAsync(string studentId) => await ViewModel.LoadAsync(studentId);
}
