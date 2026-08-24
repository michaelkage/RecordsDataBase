using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public string FormTitle => IsEditing ? "Edit student" : "Add student";
    public string SaveButtonText => IsEditing ? "Save changes" : "Add student";
    public string[] Genders { get; } = ["Male", "Female", "Other"];
    public string[] ClassLevels { get; } = ["JSS1", "JSS2", "JSS3", "SS1", "SS2", "SS3"];
    public string[] Arms { get; } = ["A", "B", "C", "D"];
    public string[] Departments { get; } = ["Science", "Arts", "Commercial", "General"];
    public string[] Statuses { get; } = ["Active", "Inactive", "Graduated", "Transferred"];

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnIsEditingChanged(bool value) { OnPropertyChanged(nameof(FormTitle)); OnPropertyChanged(nameof(SaveButtonText)); }

    [RelayCommand] private async Task LoadAsync() { IsBusy = true; try { _allStudents = await _studentService.GetAllAsync(); ApplyFilter(); StatusMessage = $"{_allStudents.Count} student{(_allStudents.Count == 1 ? "" : "s")} stored locally."; } catch (Exception ex) { StatusMessage = $"Could not load students: {ex.Message}"; } finally { IsBusy = false; } }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) { StatusMessage = "Enter the student's name."; return; }
        if (!int.TryParse(AgeText, out var age) || age is < 1 or > 100) { StatusMessage = "Enter a valid age."; return; }
        if (string.IsNullOrWhiteSpace(Gender) || string.IsNullOrWhiteSpace(ClassLevel) || string.IsNullOrWhiteSpace(Arm) || string.IsNullOrWhiteSpace(Department)) { StatusMessage = "Select gender, class, arm and department."; return; }
        IsBusy = true;
        try
        {
            Student student;
            if (IsEditing && SelectedStudent is not null) { student = SelectedStudent; student.Name = Name.Trim(); student.Age = age; student.Gender = Gender.Trim(); student.ClassLevel = ClassLevel.Trim(); await _studentService.UpdateAsync(student); StatusMessage = $"{student.Name} updated successfully."; }
            else { student = await _studentService.AddAsync(Name, age, Gender, ClassLevel); StatusMessage = $"Student added with ID {student.Id}."; }
            var data = await _dataService.LoadAsync(); var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id);
            if (details is null) { details = new StudentDetails { StudentId = student.Id }; data.StudentDetails.Add(details); }
            details.Arm = Arm.Trim(); details.Department = Department.Trim(); details.DateOfBirth = DateOfBirth.Trim(); details.AdmissionNumber = AdmissionNumber.Trim(); details.ParentName = ParentName.Trim(); details.ParentPhone = ParentPhone.Trim(); details.Address = Address.Trim(); details.Email = Email.Trim(); details.Status = Status;
            await _dataService.SaveAsync(data); await ReloadWithoutBusyMessageAsync(); ClearForm();
        }
        catch (Exception ex) { StatusMessage = $"Could not save student: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task EditAsync(Student? student)
    {
        if (student is null) return; SelectedStudent = student; Name = student.Name; AgeText = student.Age.ToString(); Gender = student.Gender; ClassLevel = student.ClassLevel;
        var data = await _dataService.LoadAsync(); var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id);
        Arm = details?.Arm ?? ""; Department = details?.Department ?? ""; DateOfBirth = details?.DateOfBirth ?? ""; AdmissionNumber = details?.AdmissionNumber ?? ""; ParentName = details?.ParentName ?? ""; ParentPhone = details?.ParentPhone ?? ""; Address = details?.Address ?? ""; Email = details?.Email ?? ""; Status = details?.Status ?? "Active";
        IsEditing = true; StatusMessage = $"Editing {student.Id}.";
    }

    [RelayCommand] private async Task DeleteAsync(Student? student) { if (student is null) return; IsBusy = true; try { await _studentService.DeleteAsync(student.Id); var data = await _dataService.LoadAsync(); data.StudentDetails.RemoveAll(d => d.StudentId == student.Id); data.Scores.RemoveAll(s => s.StudentId == student.Id); await _dataService.SaveAsync(data); StatusMessage = $"{student.Name} deleted."; await ReloadWithoutBusyMessageAsync(); ClearForm(); } catch (Exception ex) { StatusMessage = $"Could not delete student: {ex.Message}"; } finally { IsBusy = false; } }
    [RelayCommand] private void CancelEdit() { ClearForm(); StatusMessage = "Form cleared."; }
    private async Task ReloadWithoutBusyMessageAsync() { _allStudents = await _studentService.GetAllAsync(); ApplyFilter(); }
    private void ApplyFilter() { var q = SearchText.Trim(); var filtered = string.IsNullOrWhiteSpace(q) ? _allStudents : _allStudents.Where(s => s.Id.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || s.ClassLevel.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Gender.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList(); Students = new ObservableCollection<Student>(filtered); }
    private void ClearForm() { SelectedStudent = null; Name = ""; AgeText = ""; Gender = ""; ClassLevel = ""; Arm = ""; Department = ""; DateOfBirth = ""; AdmissionNumber = ""; ParentName = ""; ParentPhone = ""; Address = ""; Email = ""; Status = "Active"; IsEditing = false; }
}
