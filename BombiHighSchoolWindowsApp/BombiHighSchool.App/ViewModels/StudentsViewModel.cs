using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentsViewModel : ObservableObject
{
    private readonly StudentService _studentService = new();

    private List<Student> _allStudents = [];

    [ObservableProperty]
    private ObservableCollection<Student> students = [];

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string ageText = "";

    [ObservableProperty]
    private string gender = "";

    [ObservableProperty]
    private string classLevel = "";

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string statusMessage = "";

    [ObservableProperty]
    private bool isBusy;

    public string FormTitle => IsEditing ? "Edit student" : "Add student";
    public string SaveButtonText => IsEditing ? "Save changes" : "Add student";

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            _allStudents = await _studentService.GetAllAsync();
            ApplyFilter();
            StatusMessage = $"{_allStudents.Count} student{(_allStudents.Count == 1 ? "" : "s")} stored locally.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load students: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Enter the student's name.";
            return;
        }

        if (!int.TryParse(AgeText, out var age) || age < 1 || age > 100)
        {
            StatusMessage = "Enter a valid age.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Gender) || string.IsNullOrWhiteSpace(ClassLevel))
        {
            StatusMessage = "Select a gender and class.";
            return;
        }

        IsBusy = true;
        try
        {
            if (IsEditing && SelectedStudent is not null)
            {
                SelectedStudent.Name = Name.Trim();
                SelectedStudent.Age = age;
                SelectedStudent.Gender = Gender.Trim();
                SelectedStudent.ClassLevel = ClassLevel.Trim();
                await _studentService.UpdateAsync(SelectedStudent);
                StatusMessage = $"{SelectedStudent.Name} updated successfully.";
            }
            else
            {
                var student = await _studentService.AddAsync(Name, age, Gender, ClassLevel);
                StatusMessage = $"Student added with ID {student.Id}.";
            }

            await ReloadWithoutBusyMessageAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save student: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(Student? student)
    {
        if (student is null)
            return;

        IsBusy = true;
        try
        {
            await _studentService.DeleteAsync(student.Id);
            StatusMessage = $"{student.Name} deleted.";
            SelectedStudent = null;
            await ReloadWithoutBusyMessageAsync();
            ClearForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not delete student: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Edit(Student? student)
    {
        if (student is null)
            return;

        SelectedStudent = student;
        Name = student.Name;
        AgeText = student.Age.ToString();
        Gender = student.Gender;
        ClassLevel = student.ClassLevel;
        IsEditing = true;
        StatusMessage = $"Editing {student.Id}.";
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ClearForm();
        StatusMessage = "Edit cancelled.";
    }

    private async Task ReloadWithoutBusyMessageAsync()
    {
        _allStudents = await _studentService.GetAllAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allStudents
            : _allStudents.Where(student =>
                student.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                student.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                student.ClassLevel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                student.Gender.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        Students = new ObservableCollection<Student>(filtered);
    }

    private void ClearForm()
    {
        SelectedStudent = null;
        Name = "";
        AgeText = "";
        Gender = "";
        ClassLevel = "";
        IsEditing = false;
    }
}
