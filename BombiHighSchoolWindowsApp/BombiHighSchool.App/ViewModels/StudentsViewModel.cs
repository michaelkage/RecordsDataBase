using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentsViewModel : ObservableObject
{
    private readonly StudentService _studentService = new();
    private readonly LocalDataService _dataService = new();
    private List<Student> _allStudents = [];
    [ObservableProperty] private ObservableCollection<Student> students = [];
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string ageText = "";
    [ObservableProperty] private string gender = "";
    [ObservableProperty] private string classLevel = "";
    [ObservableProperty] private string arm = "";
    [ObservableProperty] private string department = "";
    [ObservableProperty] private string dateOfBirth = "";
    [ObservableProperty] private string admissionNumber = "";
    [ObservableProperty] private string parentName = "";
    [ObservableProperty] private string parentPhone = "";
    [ObservableProperty] private string address = "";
    [ObservableProperty] private string email = "";
    [ObservableProperty] private string status = "Active";
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isBusy;

    public string FormTitle => IsEditing ? "Edit student" : "Register student";
    public string SaveButtonText => IsEditing ? "Save changes" : "Register student";
    public Visibility OptionalDetailsVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;
    public string[] Genders { get; } = ["Male", "Female", "Other"];
    public string[] ClassLevels { get; } = ["JSS1", "JSS2", "JSS3", "SS1", "SS2", "SS3"];
    public string[] Arms => SchoolRules.Arms;
    public string[] Departments { get; } = ["Science", "Arts", "Commercial", "General"];
    public string[] Statuses { get; } = ["Active", "Inactive", "Graduated", "Transferred"];
    public bool IsSeniorClass => ClassLevel.StartsWith("SS", StringComparison.OrdinalIgnoreCase);

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnClassLevelChanged(string value) { if (!IsSeniorClass) Department = "General"; else if (Department == "General") Department = ""; OnPropertyChanged(nameof(IsSeniorClass)); }
    partial void OnIsEditingChanged(bool value) { OnPropertyChanged(nameof(FormTitle)); OnPropertyChanged(nameof(SaveButtonText)); OnPropertyChanged(nameof(OptionalDetailsVisibility)); }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try { _allStudents = await _studentService.GetAllAsync(); ApplyFilter(); StatusMessage = _dataService.LastLoadWarning ?? $"{_allStudents.Count} active student{(_allStudents.Count == 1 ? "" : "s")} stored locally."; }
        catch (DatabaseUnavailableException ex) { StatusMessage = ex.Message; }
        catch (Exception ex) { StatusMessage = $"Could not load students: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) { StatusMessage = "Enter the student's name."; return; }
        if (IsEditing && !string.IsNullOrWhiteSpace(AgeText) && (!int.TryParse(AgeText, out var parsedAge) || parsedAge is < 1 or > 100)) { StatusMessage = "Enter a valid age or leave it blank."; return; }
        if (string.IsNullOrWhiteSpace(Gender) || string.IsNullOrWhiteSpace(ClassLevel) || string.IsNullOrWhiteSpace(Arm)) { StatusMessage = "Select gender, class and arm."; return; }
        if (IsSeniorClass && string.IsNullOrWhiteSpace(Department)) { StatusMessage = "Select a department for senior students."; return; }
        IsBusy = true;
        try
        {
            if (IsEditing && SelectedStudent is not null)
            {
                var student = new Student { Id = SelectedStudent.Id, Name = Name, Age = int.TryParse(AgeText, out var parsed) ? parsed : SelectedStudent.Age, Gender = Gender, ClassLevel = ClassLevel, IsArchived = false };
                var details = new StudentDetails { StudentId = SelectedStudent.Id, Arm = Arm, Department = Department, DateOfBirth = DateOfBirth, AdmissionNumber = AdmissionNumber, ParentName = ParentName, ParentPhone = ParentPhone, Address = Address, Email = Email, Status = string.IsNullOrWhiteSpace(Status) ? "Active" : Status };
                await _studentService.UpdateProfileAsync(student, details);
                StatusMessage = $"{student.Name} updated successfully.";
            }
            else
            {
                var student = await _studentService.AddAsync(Name, Gender, ClassLevel, Arm, Department);
                var data = await _dataService.LoadAsync();
                var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id);
                StatusMessage = $"{student.Name} registered as {student.Id}. Admission number {details?.AdmissionNumber ?? "assigned"}.";
            }
            await ReloadWithoutBusyMessageAsync();
            ClearForm();
        }
        catch (DatabaseUnavailableException ex) { StatusMessage = ex.Message; }
        catch (Exception ex) { StatusMessage = $"Could not save student: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task EditAsync(Student? student)
    {
        if (student is null) return;
        try
        {
            SelectedStudent = student; Name = student.Name; AgeText = student.Age > 0 ? student.Age.ToString() : ""; Gender = student.Gender; ClassLevel = student.ClassLevel;
            var data = await _dataService.LoadAsync(); var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id);
            Arm = details?.Arm ?? ""; Department = details?.Department ?? (IsSeniorClass ? "" : "General"); DateOfBirth = details?.DateOfBirth ?? ""; AdmissionNumber = details?.AdmissionNumber ?? ""; ParentName = details?.ParentName ?? ""; ParentPhone = details?.ParentPhone ?? ""; Address = details?.Address ?? ""; Email = details?.Email ?? ""; Status = details?.Status ?? "Active";
            IsEditing = true; StatusMessage = $"Editing {student.Id}. Optional details can be completed here.";
        }
        catch (Exception ex) { StatusMessage = $"Could not load student: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task ArchiveAsync(Student? student)
    {
        if (student is null) return; IsBusy = true;
        try { await _studentService.ArchiveAsync(student.Id); StatusMessage = $"{student.Name} archived. The record was preserved and the account disabled."; await ReloadWithoutBusyMessageAsync(); ClearForm(); }
        catch (Exception ex) { StatusMessage = $"Could not archive student: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand] private void CancelEdit() { ClearForm(); StatusMessage = "Form cleared."; }
    private async Task ReloadWithoutBusyMessageAsync() { _allStudents = await _studentService.GetAllAsync(); ApplyFilter(); }
    private void ApplyFilter() { var q = SearchText.Trim(); var filtered = string.IsNullOrWhiteSpace(q) ? _allStudents : _allStudents.Where(s => s.Id.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || s.ClassLevel.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Gender.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList(); Students = new ObservableCollection<Student>(filtered); }
    private void ClearForm() { SelectedStudent = null; Name = ""; AgeText = ""; Gender = ""; ClassLevel = ""; Arm = ""; Department = ""; DateOfBirth = ""; AdmissionNumber = ""; ParentName = ""; ParentPhone = ""; Address = ""; Email = ""; Status = "Active"; IsEditing = false; }
}
