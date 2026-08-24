using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class SubjectsViewModel : ObservableObject
{
    private readonly SubjectService _service = new();
    [ObservableProperty] private ObservableCollection<Subject> subjects = [];
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string code = "";
    [ObservableProperty] private string department = "General";
    [ObservableProperty] private bool isCompulsory;
    [ObservableProperty] private Subject? selectedSubject;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isEditing;
    public string[] Departments { get; } = ["Science", "Arts", "Commercial", "General"];
    public string FormTitle => IsEditing ? "Edit subject" : "Add subject";
    public string SaveText => IsEditing ? "Save changes" : "Add subject";
    partial void OnIsEditingChanged(bool value) { OnPropertyChanged(nameof(FormTitle)); OnPropertyChanged(nameof(SaveText)); }
    [RelayCommand] public async Task LoadAsync() => Subjects = new ObservableCollection<Subject>(await _service.GetAllAsync());
    [RelayCommand] private async Task SaveAsync() { if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Code)) { StatusMessage = "Enter a subject name and code."; return; } if (IsEditing && SelectedSubject is not null) { SelectedSubject.Name = Name; SelectedSubject.Code = Code; SelectedSubject.Department = Department; SelectedSubject.IsCompulsory = IsCompulsory; await _service.UpdateAsync(SelectedSubject); StatusMessage = "Subject updated."; } else { var s = await _service.AddAsync(Name, Code, Department, IsCompulsory); StatusMessage = $"Added {s.Name} ({s.Id})."; } Clear(); await LoadAsync(); }
    [RelayCommand] private async Task DeleteAsync(Subject? subject) { if (subject is null) return; await _service.DeleteAsync(subject.Id); StatusMessage = "Subject deleted."; Clear(); await LoadAsync(); }
    [RelayCommand] private void Edit(Subject? subject) { if (subject is null) return; SelectedSubject = subject; Name = subject.Name; Code = subject.Code; Department = subject.Department; IsCompulsory = subject.IsCompulsory; IsEditing = true; }
    [RelayCommand] private void Clear() { SelectedSubject = null; Name = ""; Code = ""; Department = "General"; IsCompulsory = false; IsEditing = false; }
}
