using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentPortalViewModel : ObservableObject
{
    private readonly StudentPortalService _service = new();

    [ObservableProperty] private StudentPortalSnapshot? snapshot;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? studentId;

    public ObservableCollection<StudentScoreRow> Scores { get; } = [];
    public string StudentName => Snapshot?.Student.Name ?? "Student";
    public string ClassLabel => Snapshot is null ? "" : $"{Snapshot.Student.ClassLevel} {Snapshot.Details.Arm}".Trim();
    public string AverageText => Snapshot is null ? "0.00%" : $"{Snapshot.Average:0.00}%";
    public string ClassPositionText => Snapshot is null || Snapshot.Position == 0 ? "—" : $"{Snapshot.Position} / {Snapshot.ClassSize}";
    public string DepartmentPositionText => Snapshot is null || Snapshot.DepartmentPosition == 0 ? "—" : $"{Snapshot.DepartmentPosition} / {Snapshot.DepartmentSize}";

    public async Task LoadAsync(string id)
    {
        StudentId = id;
        IsBusy = true;
        try
        {
            Snapshot = await _service.GetSnapshotAsync(id);
            Scores.Clear();
            if (Snapshot is not null) foreach (var score in Snapshot.Scores) Scores.Add(score);
            StatusMessage = Snapshot is null ? "Student record not found." : $"Welcome back, {Snapshot.Student.Name}.";
            NotifyCalculatedProperties();
        }
        catch (Exception ex) { StatusMessage = $"Could not load portal: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (!string.IsNullOrWhiteSpace(StudentId)) await LoadAsync(StudentId);
    }

    private void NotifyCalculatedProperties()
    {
        OnPropertyChanged(nameof(StudentName)); OnPropertyChanged(nameof(ClassLabel)); OnPropertyChanged(nameof(AverageText));
        OnPropertyChanged(nameof(ClassPositionText)); OnPropertyChanged(nameof(DepartmentPositionText));
    }
}
