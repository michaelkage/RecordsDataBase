using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class StudentLoginViewModel : ObservableObject
{
    private readonly AuthenticationService _authenticationService;

    [ObservableProperty] private string studentId = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isBusy;

    public event EventHandler? LoginSucceeded;

    public StudentLoginViewModel(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    }

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
                StatusMessage = "Invalid student credentials, or the account is disabled.";
                return;
            }

            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (DatabaseUnavailableException)
        {
            StatusMessage = "The school database could not be opened. Please try again later.";
        }
        catch (Exception)
        {
            StatusMessage = "Sign-in failed. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
