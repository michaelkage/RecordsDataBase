using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentTranscriptViewModel : ObservableObject
{
    private readonly TranscriptService _service = new();
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string id = "";
    [ObservableProperty] private string @class = "";
    [ObservableProperty] private string department = "";
    [ObservableProperty] private string average = "0.00%";
    [ObservableProperty] private string statusMessage = "";
    public ObservableCollection<TranscriptRow> Rows { get; } = [];

    public async Task LoadAsync(string studentId)
    {
        var result = await _service.BuildAsync(studentId);
        if (result is null) { StatusMessage = "Student record not found."; return; }
        Name = result.Value.Student.Name;
        Id = result.Value.Student.Id;
        Class = $"{result.Value.Student.ClassLevel} {result.Value.Details.Arm}".Trim();
        Department = result.Value.Details.Department;
        Rows.Clear(); foreach (var row in result.Value.Rows) Rows.Add(row);
        Average = $"{(Rows.Count == 0 ? 0 : Rows.Average(x => x.Total)):0.00}%";
        StatusMessage = "Transcript ready for Windows printing.";
    }
}
