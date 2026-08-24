using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthenticationService _authenticationService = new();

    [ObservableProperty] private string username = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isBusy;

    public event EventHandler? LoginSucceeded;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Enter your username and password.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Signing in...";
        try
        {
            await _authenticationService.InitializeAsync();
            var account = await _authenticationService.AuthenticateAdminAsync(Username, Password);

            if (account is null)
            {
                StatusMessage = "Invalid administrator credentials.";
                return;
            }

            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sign-in failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
