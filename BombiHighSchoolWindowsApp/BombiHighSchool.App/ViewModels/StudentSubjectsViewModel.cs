using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentSubjectsViewModel : ObservableObject
{
    private readonly LocalDataService _data = new();
    private readonly EnrollmentService _enrollments = new();
    [ObservableProperty] private ObservableCollection<Subject> enrolledSubjects = [];
    [ObservableProperty] private string statusMessage = "";
    private string? _studentId;

    public async Task LoadAsync(string studentId)
    {
        _studentId = studentId;
        try
        {
            await _enrollments.EnsureCompulsorySubjectsAsync(studentId);
            EnrolledSubjects = new(await _enrollments.GetForStudentAsync(studentId));
            StatusMessage = "Subjects are managed by the school administrator.";
        }
        catch (Exception ex) { StatusMessage = $"Could not load subjects: {ex.Message}"; }
    }
}
