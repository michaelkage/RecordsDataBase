using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class ScoresViewModel : ObservableObject
{
    private readonly LocalDataService _data = new();
    private readonly ScoreService _service;
    private List<Subject> _allSubjects = [];
    private List<SubjectEnrollment> _enrollments = [];
    private int _subjectLoadVersion;
    [ObservableProperty] private ObservableCollection<Student> students = [];
    [ObservableProperty] private ObservableCollection<Subject> subjects = [];
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Subject? selectedSubject;
    [ObservableProperty] private string testText = "";
    [ObservableProperty] private string examText = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private double currentTotal;
    [ObservableProperty] private string currentGrade = "—";
    [ObservableProperty] private bool isBusy;

    public ScoresViewModel() => _service = new ScoreService(_data);
    partial void OnTestTextChanged(string value) => Recalculate();
    partial void OnExamTextChanged(string value) => Recalculate();
    partial void OnSelectedStudentChanged(Student? value)
    {
        _subjectLoadVersion++;
        SelectedSubject = null; TestText = ""; ExamText = "";
        if (value is null) { Subjects = new(); return; }
        var ids = _enrollments.Where(e => e.StudentId.Equals(value.Id, StringComparison.OrdinalIgnoreCase)).Select(e => e.SubjectId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Subjects = new ObservableCollection<Subject>(_allSubjects.Where(s => ids.Contains(s.Id)));
        StatusMessage = Subjects.Count == 0 ? "This student has no enrolled subjects yet. Enroll the student before entering scores." : $"{Subjects.Count} enrolled subject{(Subjects.Count == 1 ? "" : "s")} available.";
    }
    partial void OnSelectedSubjectChanged(Subject? value)
    {
        TestText = ""; ExamText = "";
        var version = ++_subjectLoadVersion;
        _ = LoadSelectedSubjectAsync(value, version);
    }
    private async Task LoadSelectedSubjectAsync(Subject? value, int version)
    {
        var student = SelectedStudent;
        if (student is null || value is null) { Recalculate(); return; }
        try
        {
            var period = await _data.LoadAsync();
            if (version != _subjectLoadVersion || SelectedStudent?.Id != student.Id || SelectedSubject?.Id != value.Id) return;
            var score = period.Scores.FirstOrDefault(s => s.StudentId.Equals(student.Id, StringComparison.OrdinalIgnoreCase) && s.SubjectId.Equals(value.Id, StringComparison.OrdinalIgnoreCase) && s.Session.Equals(period.CurrentAcademicPeriod.Session, StringComparison.OrdinalIgnoreCase) && s.Term.Equals(period.CurrentAcademicPeriod.Term, StringComparison.OrdinalIgnoreCase));
            if (score is not null) { TestText = score.ScoreValue.ToString("0.#"); ExamText = score.ExamScore.ToString("0.#"); StatusMessage = $"Existing {period.CurrentAcademicPeriod.Term} result loaded. Edit and save to update it."; }
            else StatusMessage = "No result exists for this student and subject in the current academic period.";
        }
        catch (Exception ex) { if (version == _subjectLoadVersion) StatusMessage = $"Could not load the existing score: {ex.Message}"; }
    }
    private void Recalculate()
    {
        double.TryParse(TestText, out var t); double.TryParse(ExamText, out var e);
        var total = Math.Clamp(t, 0, 40) + Math.Clamp(e, 0, 60); CurrentTotal = total;
        CurrentGrade = total switch { >= 75 => "A", >= 70 => "B", >= 60 => "C", >= 50 => "D", >= 45 => "E", _ => "F" };
    }
    [RelayCommand] public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var d = await _data.LoadAsync();
            Students = new(d.Students.Where(s => !s.IsArchived && d.StudentDetails.FirstOrDefault(x => x.StudentId == s.Id)?.Status != "Archived"));
            _allSubjects = d.Subjects.ToList(); _enrollments = d.Enrollments.ToList(); Subjects = new(); SelectedStudent = null; SelectedSubject = null; TestText = ExamText = "";
            StatusMessage = $"Ready for {d.CurrentAcademicPeriod.Session} • {d.CurrentAcademicPeriod.Term}. Select a student to see only their enrolled subjects.";
        }
        catch (Exception ex) { StatusMessage = $"Could not load scores: {ex.Message}"; }
        finally { IsBusy = false; }
    }
    private async Task<bool> SaveCurrentAsync()
    {
        if (SelectedStudent is null) { StatusMessage = "Select a student first."; return false; }
        if (SelectedSubject is null) { StatusMessage = "Select an enrolled subject first."; return false; }
        if (!double.TryParse(TestText, out var t) || !double.TryParse(ExamText, out var e) || !double.IsFinite(t) || !double.IsFinite(e) || t < 0 || t > 40 || e < 0 || e > 60) { StatusMessage = "Test must be 0–40 and exam must be 0–60."; return false; }
        await _service.SaveAsync(SelectedStudent.Id, SelectedSubject.Id, t, e);
        StatusMessage = $"Saved {SelectedStudent.Name}: {SelectedSubject.Name} — {CurrentTotal:0.#} ({CurrentGrade})."; return true;
    }
    [RelayCommand] private async Task SaveAsync() { IsBusy = true; try { await SaveCurrentAsync(); } catch (Exception ex) { StatusMessage = $"Could not save score: {ex.Message}"; } finally { IsBusy = false; } }
    [RelayCommand] private async Task SaveAndNextAsync()
    {
        IsBusy = true;
        try { if (!await SaveCurrentAsync()) return; var index = SelectedStudent is null ? -1 : Students.IndexOf(SelectedStudent); if (index >= 0 && index + 1 < Students.Count) { SelectedStudent = Students[index + 1]; StatusMessage += " Next student selected."; } else StatusMessage += " Reached the end of the student list."; }
        catch (Exception ex) { StatusMessage = $"Could not save score: {ex.Message}"; }
        finally { IsBusy = false; }
    }
    [RelayCommand] private void Clear() { TestText = ""; ExamText = ""; StatusMessage = "Entry cleared."; }
}
