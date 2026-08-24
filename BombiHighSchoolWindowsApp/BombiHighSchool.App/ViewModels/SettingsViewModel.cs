using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Services;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BombiHighSchool.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AuthenticationService _authenticationService = new();
    private readonly LocalDataService _dataService = new();
    [ObservableProperty] private string currentPassword = "";
    [ObservableProperty] private string newPassword = "";
    [ObservableProperty] private string confirmPassword = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    private async Task ChangeAdminPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword)) { StatusMessage = "Enter the current and new passwords."; return; }
        if (NewPassword != ConfirmPassword) { StatusMessage = "The new passwords do not match."; return; }
        IsBusy = true;
        try { await _authenticationService.ChangePasswordAsync("admin", CurrentPassword, NewPassword); CurrentPassword = NewPassword = ConfirmPassword = ""; StatusMessage = "Administrator password changed successfully."; }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        IsBusy = true;
        try { await _dataService.CreateBackupAsync(); StatusMessage = $"Backup created: {_dataService.BackupPath}"; }
        catch (Exception ex) { StatusMessage = $"Backup failed: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        IsBusy = true;
        try { await _dataService.RestoreBackupAsync(); StatusMessage = "Backup restored successfully. Reload the current page to see restored records."; }
        catch (Exception ex) { StatusMessage = $"Restore failed: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ExportDatabaseAsync()
    {
        if (App.MainWindow is null) { StatusMessage = "The application window is unavailable."; return; }
        try
        {
            var picker = new FileSavePicker();
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
            picker.SuggestedFileName = $"BombiHighSchool-Database-{DateTime.Now:yyyyMMdd-HHmm}.json";
            picker.FileTypeChoices.Add("Bombi High School database", new List<string> { ".json" });
            var file = await picker.PickSaveFileAsync();
            if (file is null) return;
            IsBusy = true;
            await _dataService.ExportDatabaseAsync(file.Path);
            StatusMessage = $"Database exported to {file.Path}";
        }
        catch (Exception ex) { StatusMessage = $"Export failed: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ImportDatabaseAsync()
    {
        if (App.MainWindow is null) { StatusMessage = "The application window is unavailable."; return; }
        try
        {
            var picker = new FileOpenPicker();
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));
            picker.FileTypeFilter.Add(".json");
            var file = await picker.PickSingleFileAsync();
            if (file is null) return;
            IsBusy = true;
            await _dataService.ImportDatabaseAsync(file.Path);
            StatusMessage = "Database imported successfully. Reload the current page to use the imported records.";
        }
        catch (Exception ex) { StatusMessage = $"Import failed: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}
