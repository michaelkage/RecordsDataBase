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
        var data = await _data.LoadAsync();
        var enrolled = await _enrollments.GetForStudentAsync(studentId);
        EnrolledSubjects = new(enrolled);
        var ids = enrolled.Select(x => x.Id).ToHashSet();
        AvailableSubjects = new(data.Subjects.Where(x => !ids.Contains(x.Id)).OrderBy(x => x.Name));
    }

    [RelayCommand]
    private async Task EnrollAsync()
    {
        if (_studentId is null || SelectedAvailableSubject is null) { StatusMessage = "Select a subject."; return; }
        await _enrollments.EnrollAsync(_studentId, SelectedAvailableSubject.Id);
        StatusMessage = $"{SelectedAvailableSubject.Name} enrolled.";
        await LoadAsync(_studentId);
    }

    [RelayCommand]
    private async Task RemoveAsync()
    {
        if (_studentId is null || SelectedEnrolledSubject is null) { StatusMessage = "Select an enrolled subject."; return; }
        await _enrollments.RemoveAsync(_studentId, SelectedEnrolledSubject.Id);
        StatusMessage = $"{SelectedEnrolledSubject.Name} removed.";
        await LoadAsync(_studentId);
    }
}
