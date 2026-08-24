using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class ScoresViewModel : ObservableObject
{
    private readonly LocalDataService _data = new();
    private readonly ScoreService _service = new();
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

    partial void OnTestTextChanged(string value) => Recalculate();
    partial void OnExamTextChanged(string value) => Recalculate();
    private void Recalculate()
    {
        double.TryParse(TestText, out var t); double.TryParse(ExamText, out var e);
        var total = Math.Clamp(t, 0, 40) + Math.Clamp(e, 0, 60);
        CurrentTotal = total;
        CurrentGrade = total switch { >=75=>"A", >=70=>"B", >=60=>"C", >=50=>"D", >=45=>"E", _=>"F" };
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try { var d = await _data.LoadAsync(); Students = new(d.Students.Where(s => !s.IsArchived)); Subjects = new(d.Subjects); StatusMessage = "Ready. Enter a result, then use Save & next for fast entry."; }
        catch (Exception ex) { StatusMessage = $"Could not load scores: {ex.Message}"; }
    }

    private async Task<bool> SaveCurrentAsync()
    {
        if (SelectedStudent is null || SelectedSubject is null) { StatusMessage = "Select a student and subject."; return false; }
        if (!double.TryParse(TestText, out var t) || !double.TryParse(ExamText, out var e) || t < 0 || t > 40 || e < 0 || e > 60) { StatusMessage = "Test must be 0–40 and exam must be 0–60."; return false; }
        await _service.SaveAsync(SelectedStudent.Id, SelectedSubject.Id, t, e);
        StatusMessage = $"Saved {SelectedStudent.Name}: {SelectedSubject.Name} — {CurrentTotal:0.#} ({CurrentGrade}).";
        return true;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        try { await SaveCurrentAsync(); }
        catch (Exception ex) { StatusMessage = $"Could not save score: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAndNextAsync()
    {
        IsBusy = true;
        try
        {
            if (!await SaveCurrentAsync()) return;
            var index = SelectedStudent is null ? -1 : Students.IndexOf(SelectedStudent);
            if (index >= 0 && index + 1 < Students.Count) { SelectedStudent = Students[index + 1]; TestText = ""; ExamText = ""; StatusMessage += " Next student selected."; }
            else { TestText = ""; ExamText = ""; StatusMessage += " Reached the end of the student list."; }
        }
        catch (Exception ex) { StatusMessage = $"Could not save score: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Clear() { TestText = ""; ExamText = ""; StatusMessage = "Entry cleared."; }
}
