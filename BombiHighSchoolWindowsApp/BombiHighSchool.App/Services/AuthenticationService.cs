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
        await _dataService.UpdateAsync(current =>
        {
            current.Users.Add(new UserAccount
            {
                Username = FailSafeAdminUsername,
                Role = "Admin",
                PasswordHash = hash,
                PasswordSalt = salt,
                MustChangePassword = true,
                IsEnabled = true
            });
            return Task.CompletedTask;
        });
    }

    public async Task<UserAccount?> AuthenticateAdminAsync(string username, string password)
    {
        try
        {
            // The database is always authoritative when it is available, including disabled accounts.
            return await AuthenticateAsync(username, password, "Admin");
        }
        catch (DatabaseUnavailableException)
        {
            // Recovery credential is deliberately available ONLY when the local database cannot be read.
            // It never bypasses a valid/disabled database account.
            if (username.Trim().Equals(FailSafeAdminUsername, StringComparison.OrdinalIgnoreCase) && password == FailSafeAdminPassword)
            {
                return new UserAccount
                {
                    Username = FailSafeAdminUsername,
                    Role = "Admin",
                    IsEnabled = true,
                    MustChangePassword = true
                };
            }
            return null;
        }
    }

    public Task<UserAccount?> AuthenticateStudentAsync(string studentId, string password) => AuthenticateAsync(studentId, password, "Student");

    public async Task ChangePasswordAsync(string username, string currentPassword, string newPassword)
    {
        if (newPassword.Length < 8) throw new ArgumentException("The new password must contain at least 8 characters.");
        await _dataService.UpdateAsync(data =>
        {
            var account = data.Users.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
            if (account is null || !account.IsEnabled || !PasswordHasher.Verify(currentPassword, account.PasswordHash, account.PasswordSalt)) throw new InvalidOperationException("The current password is incorrect.");
            var (hash, salt) = PasswordHasher.Hash(newPassword);
            account.PasswordHash = hash;
            account.PasswordSalt = salt;
            account.MustChangePassword = false;
            return Task.CompletedTask;
        });
    }

    public async Task SetStudentPasswordAsync(string studentId, string password)
    {
        if (password.Length < 8) throw new ArgumentException("The password must contain at least 8 characters.");
        await _dataService.UpdateAsync(data =>
        {
            var account = data.Users.FirstOrDefault(u => u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
            if (account is null) throw new InvalidOperationException("Student account not found.");
            var (hash, salt) = PasswordHasher.Hash(password);
            account.PasswordHash = hash;
            account.PasswordSalt = salt;
            account.MustChangePassword = true;
            return Task.CompletedTask;
        });
    }

    public async Task SetStudentEnabledAsync(string studentId, bool enabled)
    {
        await _dataService.UpdateAsync(data =>
        {
            var account = data.Users.FirstOrDefault(u => u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
            if (account is null) throw new InvalidOperationException("Student account not found.");
            account.IsEnabled = enabled;
            return Task.CompletedTask;
        });
    }

    public async Task EnsureStudentAccountAsync(string studentId, string initialPassword)
    {
        var existing = (await _dataService.LoadAsync()).Users.FirstOrDefault(u => u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) { await SetStudentEnabledAsync(studentId, true); return; }
        var (hash, salt) = PasswordHasher.Hash(initialPassword);
        await _dataService.UpdateAsync(data =>
        {
            if (!data.Users.Any(u => u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase)))
            {
                data.Users.Add(new UserAccount { Username = studentId, StudentId = studentId, Role = "Student", PasswordHash = hash, PasswordSalt = salt, MustChangePassword = true, IsEnabled = true });
            }
            return Task.CompletedTask;
        });
    }

    private async Task<UserAccount?> AuthenticateAsync(string username, string password, string role)
    {
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase) && u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        return account is not null && account.IsEnabled && PasswordHasher.Verify(password, account.PasswordHash, account.PasswordSalt) ? account : null;
    }
}
