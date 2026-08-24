using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentResultsViewModel : ObservableObject
{
    private readonly StudentPortalService _service = new();
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private string averageText = "0.00%";
    [ObservableProperty] private string classPositionText = "—";
    [ObservableProperty] private string departmentPositionText = "—";
    [ObservableProperty] private ObservableCollection<StudentScoreRow> scores = [];

    public async Task LoadAsync(string studentId)
    {
        var snapshot = await _service.GetSnapshotAsync(studentId);
        if (snapshot is null) { StatusMessage = "Student record not found."; return; }
        AverageText = $"{snapshot.Average:0.00}%";
        ClassPositionText = snapshot.Position == 0 ? "—" : $"{snapshot.Position} / {snapshot.ClassSize}";
        DepartmentPositionText = snapshot.DepartmentPosition == 0 ? "—" : $"{snapshot.DepartmentPosition} / {snapshot.DepartmentSize}";
        Scores = new(snapshot.Scores);
        StatusMessage = "Results loaded from the local Windows database.";
    }
}
