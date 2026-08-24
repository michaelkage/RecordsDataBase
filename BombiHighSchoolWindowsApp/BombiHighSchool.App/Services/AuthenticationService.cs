using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class AuthenticationService
{
    private const string FailSafeAdminUsername = "admin";
    private const string FailSafeAdminPassword = "Admin@1234";
    private readonly LocalDataService _dataService;

    public AuthenticationService(LocalDataService? dataService = null) { _dataService = dataService ?? new LocalDataService(); }

    public async Task InitializeAsync()
    {
        var data = await _dataService.LoadAsync();
        if (data.Users.Any(u => u.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))) return;
        var (hash, salt) = PasswordHasher.Hash(FailSafeAdminPassword);
        data.Users.Add(new UserAccount { Username = FailSafeAdminUsername, Role = "Admin", PasswordHash = hash, PasswordSalt = salt, MustChangePassword = true, IsEnabled = true });
        await _dataService.SaveAsync(data);
    }

    public async Task<UserAccount?> AuthenticateAdminAsync(string username, string password)
    {
        var account = await AuthenticateAsync(username, password, "Admin");
        if (account is not null) return account;
        // Last-resort recovery credential. Normal authentication is always backed by the local DB.
        if (username.Trim().Equals(FailSafeAdminUsername, StringComparison.OrdinalIgnoreCase) && password == FailSafeAdminPassword)
            return new UserAccount { Username = FailSafeAdminUsername, Role = "Admin", IsEnabled = true, MustChangePassword = true };
        return null;
    }

    public Task<UserAccount?> AuthenticateStudentAsync(string studentId, string password) => AuthenticateAsync(studentId, password, "Student");

    public async Task ChangePasswordAsync(string username, string currentPassword, string newPassword)
    {
        if (newPassword.Length < 8) throw new ArgumentException("The new password must contain at least 8 characters.");
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        if (account is null || !account.IsEnabled || !PasswordHasher.Verify(currentPassword, account.PasswordHash, account.PasswordSalt)) throw new InvalidOperationException("The current password is incorrect.");
        var (hash, salt) = PasswordHasher.Hash(newPassword);
        account.PasswordHash = hash; account.PasswordSalt = salt; account.MustChangePassword = false;
        await _dataService.SaveAsync(data);
    }

    public async Task SetStudentPasswordAsync(string studentId, string password)
    {
        if (password.Length < 8) throw new ArgumentException("The password must contain at least 8 characters.");
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u => u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
        if (account is null) throw new InvalidOperationException("Student account not found.");
        var (hash, salt) = PasswordHasher.Hash(password); account.PasswordHash = hash; account.PasswordSalt = salt; account.MustChangePassword = true;
        await _dataService.SaveAsync(data);
    }

    public async Task SetStudentEnabledAsync(string studentId, bool enabled)
    {
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u => u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
        if (account is null) throw new InvalidOperationException("Student account not found.");
        account.IsEnabled = enabled;
        await _dataService.SaveAsync(data);
    }

    public async Task EnsureStudentAccountAsync(string studentId, string initialPassword)
    {
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u => u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
        if (account is not null) { account.IsEnabled = true; await _dataService.SaveAsync(data); return; }
        var (hash, salt) = PasswordHasher.Hash(initialPassword);
        data.Users.Add(new UserAccount { Username = studentId, StudentId = studentId, Role = "Student", PasswordHash = hash, PasswordSalt = salt, MustChangePassword = true, IsEnabled = true });
        await _dataService.SaveAsync(data);
    }

    private async Task<UserAccount?> AuthenticateAsync(string username, string password, string role)
    {
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase) && u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        return account is not null && account.IsEnabled && PasswordHasher.Verify(password, account.PasswordHash, account.PasswordSalt) ? account : null;
    }
}
