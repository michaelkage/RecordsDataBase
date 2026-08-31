using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentProfileViewModel : ObservableObject
{
    private readonly StudentPortalService _portal = new();
    private readonly TranscriptService _transcripts = new();
    private readonly TranscriptExportService _export = new();
    private string? _studentId;

    [ObservableProperty] private StudentPortalSnapshot? snapshot;
    [ObservableProperty] private string statusMessage = "";

    public string Name => Snapshot?.Student.Name ?? "";
    public string Id => Snapshot?.Student.Id ?? "";
    public string Class => Snapshot is null ? "" : $"{Snapshot.Student.ClassLevel} {Snapshot.Details.Arm}".Trim();
    public string Department => Snapshot?.Details.Department ?? "";
    public string AdmissionNumber => Snapshot?.Details.AdmissionNumber ?? "";
    public string ParentName => Snapshot?.Details.ParentName ?? "";
    public string ParentPhone => Snapshot?.Details.ParentPhone ?? "";
    public string Email => Snapshot?.Details.Email ?? "";
    public string Address => Snapshot?.Details.Address ?? "";
    public string Average => Snapshot is null ? "0.00%" : $"{Snapshot.Average:0.00}%";

    public async Task LoadAsync(string studentId)
    {
        _studentId = studentId;
        Snapshot = await _portal.GetSnapshotAsync(studentId);
        StatusMessage = Snapshot is null ? "Student record not found." : "Profile loaded from local data.";
        Notify();
    }

    [RelayCommand]
    private async Task ExportTranscriptAsync()
    {
        if (string.IsNullOrWhiteSpace(_studentId)) return;
        var transcript = await _transcripts.BuildAsync(_studentId);
        if (transcript is null) { StatusMessage = "Could not build transcript."; return; }
        var text = _export.BuildText(transcript.Value.Student, transcript.Value.Details, transcript.Value.Rows);
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(folder, $"{transcript.Value.Student.Id}-Transcript.txt");
        await File.WriteAllTextAsync(path, text);
        StatusMessage = $"Transcript exported to Documents: {Path.GetFileName(path)}";
    }

    private void Notify()
    {
        OnPropertyChanged(nameof(Name)); OnPropertyChanged(nameof(Id)); OnPropertyChanged(nameof(Class));
        OnPropertyChanged(nameof(Department)); OnPropertyChanged(nameof(AdmissionNumber)); OnPropertyChanged(nameof(ParentName));
        OnPropertyChanged(nameof(ParentPhone)); OnPropertyChanged(nameof(Email)); OnPropertyChanged(nameof(Address)); OnPropertyChanged(nameof(Average));
    }
}
