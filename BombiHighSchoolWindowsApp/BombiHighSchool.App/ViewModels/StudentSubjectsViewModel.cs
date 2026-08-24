using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentSubjectsViewModel : ObservableObject
{
    private readonly LocalDataService _data = new();
    private readonly EnrollmentService _enrollments = new();
    [ObservableProperty] private ObservableCollection<Subject> availableSubjects = [];
    [ObservableProperty] private ObservableCollection<Subject> enrolledSubjects = [];
    [ObservableProperty] private Subject? selectedAvailableSubject;
    [ObservableProperty] private Subject? selectedEnrolledSubject;
    [ObservableProperty] private string statusMessage = "";
    private string? _studentId;

    public async Task LoadAsync(string studentId)
    {
        _studentId = studentId;
        try
        {
            await _enrollments.EnsureCompulsorySubjectsAsync(studentId);
            var data = await _data.LoadAsync();
            var enrolled = await _enrollments.GetForStudentAsync(studentId);
            EnrolledSubjects = new(enrolled);
            var ids = enrolled.Select(x => x.Id).ToHashSet();
            AvailableSubjects = new(data.Subjects.Where(x => !ids.Contains(x.Id)).OrderBy(x => x.Name));
            StatusMessage = "Subject enrollment is controlled by the school administrator.";
        }
        catch (Exception ex) { StatusMessage = $"Could not load subjects: {ex.Message}"; }
    }

    [RelayCommand]
    private void Enroll() => StatusMessage = "Students cannot enroll subjects directly. Ask the school administrator to assign the subject.";

    [RelayCommand]
    private void Remove() => StatusMessage = "Students cannot remove subjects directly. Ask the school administrator to change the enrollment.";
}
