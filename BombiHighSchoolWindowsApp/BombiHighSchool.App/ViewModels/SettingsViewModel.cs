using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AuthenticationService _authenticationService = new();
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
        try
        {
            await _authenticationService.ChangePasswordAsync("admin", CurrentPassword, NewPassword);
            CurrentPassword = NewPassword = ConfirmPassword = "";
            StatusMessage = "Administrator password changed successfully.";
        }
        catch (Exception ex) { StatusMessage = ex.Message; }
        finally { IsBusy = false; }
    }
}
