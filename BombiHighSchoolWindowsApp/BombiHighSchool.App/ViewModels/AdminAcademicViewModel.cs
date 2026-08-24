using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class AdminAcademicViewModel : ObservableObject
{
    private readonly LocalDataService _data = new();
    private readonly EnrollmentService _enrollments = new();
    private readonly ScoreService _scores = new();
    [ObservableProperty] private ObservableCollection<Student> students = [];
    [ObservableProperty] private ObservableCollection<Subject> subjects = [];
    [ObservableProperty] private ObservableCollection<Subject> enrolledSubjects = [];
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Subject? selectedSubject;
    [ObservableProperty] private string caText = "";
    [ObservableProperty] private string examText = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private double total;
    [ObservableProperty] private string grade = "—";

    partial void OnCaTextChanged(string value) => Calculate();
    partial void OnExamTextChanged(string value) => Calculate();
    partial void OnSelectedStudentChanged(Student? value) => _ = LoadStudentSubjectsAsync(value);

    private void Calculate()
    {
        double.TryParse(CaText, out var ca); double.TryParse(ExamText, out var exam);
        Total = Math.Clamp(ca, 0, 40) + Math.Clamp(exam, 0, 60);
        Grade = Total switch { >= 75 => "A", >= 70 => "B", >= 60 => "C", >= 50 => "D", >= 45 => "E", _ => "F" };
    }

    public async Task LoadAsync()
    {
        var data = await _data.LoadAsync();
        Students = new(data.Students.OrderBy(s => s.Name));
        Subjects = new(data.Subjects.OrderBy(s => s.Name));
        if (SelectedStudent is not null) await LoadStudentSubjectsAsync(SelectedStudent);
    }

    private async Task LoadStudentSubjectsAsync(Student? student)
    {
        if (student is null) { EnrolledSubjects.Clear(); return; }
        EnrolledSubjects = new(await _enrollments.GetForStudentAsync(student.Id));
        SelectedSubject = EnrolledSubjects.FirstOrDefault();
        StatusMessage = $"{student.Name}: {EnrolledSubjects.Count} enrolled subject(s).";
    }

    [RelayCommand]
    private async Task EnrollAsync()
    {
        if (SelectedStudent is null || SelectedSubject is null) { StatusMessage = "Select a student and subject."; return; }
        await _enrollments.EnrollAsync(SelectedStudent.Id, SelectedSubject.Id);
        await LoadStudentSubjectsAsync(SelectedStudent);
        StatusMessage = $"{SelectedSubject.Name} enrolled for {SelectedStudent.Name}.";
    }

    [RelayCommand]
    private async Task RemoveAsync()
    {
        if (SelectedStudent is null || SelectedSubject is null) { StatusMessage = "Select a student and enrolled subject."; return; }
        await _enrollments.RemoveAsync(SelectedStudent.Id, SelectedSubject.Id);
        await LoadStudentSubjectsAsync(SelectedStudent);
        StatusMessage = $"{SelectedSubject.Name} removed.";
    }

    [RelayCommand]
    private async Task SaveScoreAsync()
    {
        if (SelectedStudent is null || SelectedSubject is null) { StatusMessage = "Select a student and enrolled subject."; return; }
        if (!double.TryParse(CaText, out var ca) || !double.TryParse(ExamText, out var exam) || ca is < 0 or > 40 || exam is < 0 or > 60) { StatusMessage = "CA must be 0–40 and exam must be 0–60."; return; }
        if (!EnrolledSubjects.Any(s => s.Id == SelectedSubject.Id)) { StatusMessage = "Enroll the subject before entering a score."; return; }
        await _scores.SaveAsync(SelectedStudent.Id, SelectedSubject.Id, ca, exam);
        StatusMessage = $"Saved {SelectedStudent.Name} / {SelectedSubject.Name}: {Total:0.#} ({Grade}).";
    }
}
