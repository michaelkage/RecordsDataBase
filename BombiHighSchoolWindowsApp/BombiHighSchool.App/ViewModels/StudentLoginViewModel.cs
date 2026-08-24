using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentLoginViewModel : ObservableObject
{
    private readonly AuthenticationService _authenticationService = new();

    [ObservableProperty] private string studentId = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isBusy;

    public event EventHandler? LoginSucceeded;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(StudentId) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Enter your BHS ID and password.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Signing in...";
        try
        {
            var account = await _authenticationService.AuthenticateStudentAsync(StudentId, Password);
            if (account is null)
            {
                StatusMessage = "Invalid student credentials, or no student account exists.";
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
