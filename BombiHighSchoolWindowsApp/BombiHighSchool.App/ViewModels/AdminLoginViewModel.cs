using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BombiHighSchool.App.Services;

namespace BombiHighSchool.App.ViewModels;

public partial class AdminLoginViewModel : ObservableObject
{
    private readonly AuthenticationService _authenticationService;

    [ObservableProperty] private string username = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isBusy;

    public event EventHandler? LoginSucceeded;

    public AdminLoginViewModel(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Enter your username and password.";
            return;
        }

        IsBusy = true;
        StatusMessage = "";
        try
        {
            await _authenticationService.InitializeAsync();
            var account = await _authenticationService.AuthenticateAdminAsync(Username, Password);
            if (account is null)
            {
                StatusMessage = "Invalid administrator credentials or the account is disabled.";
                return;
            }

            SessionService.StartAdmin(account.Username);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (DatabaseUnavailableException)
        {
            StatusMessage = "The school database could not be opened. Check the database status in Settings.";
        }
        catch (Exception)
        {
            StatusMessage = "Sign-in failed. Please try again or check the database status.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
