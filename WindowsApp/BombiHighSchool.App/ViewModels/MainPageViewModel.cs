using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Models;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly LocalDataService _dataService = new();

    [ObservableProperty]
    private string currentSection = "Dashboard";

    [ObservableProperty]
    private int studentCount;

    [ObservableProperty]
    private int subjectCount;

    [ObservableProperty]
    private string storageStatus = "Loading local database…";

    [ObservableProperty]
    private string databaseLocation = "";

    public MainPageViewModel()
    {
        DatabaseLocation = _dataService.DatabasePath;
    }

    [RelayCommand]
    private void Navigate(string section)
    {
        CurrentSection = section;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        var data = await _dataService.LoadAsync();

        StudentCount = data.Students.Count;
        SubjectCount = 0;
        StorageStatus = "Local database ready";
    }
}
