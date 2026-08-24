using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class AuthenticationService
{
    private readonly LocalDataService _dataService;

    public AuthenticationService(LocalDataService? dataService = null)
    {
        _dataService = dataService ?? new LocalDataService();
    }

    public async Task InitializeAsync()
    {
        var data = await _dataService.LoadAsync();

        if (data.Users.Any(u => u.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            return;

        var (hash, salt) = PasswordHasher.Hash("Admin@1234");
        data.Users.Add(new UserAccount
        {
            Username = "admin",
            Role = "Admin",
            PasswordHash = hash,
            PasswordSalt = salt,
            MustChangePassword = true
        });

        await _dataService.SaveAsync(data);
    }

    public async Task<UserAccount?> AuthenticateAdminAsync(string username, string password)
        => await AuthenticateAsync(username, password, "Admin");

    public async Task<UserAccount?> AuthenticateStudentAsync(string studentId, string password)
        => await AuthenticateAsync(studentId, password, "Student");

    public async Task SetStudentPasswordAsync(string studentId, string password)
    {
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u =>
            u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
            u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));

        if (account is null)
            throw new InvalidOperationException("Student account not found.");

        var (hash, salt) = PasswordHasher.Hash(password);
        account.PasswordHash = hash;
        account.PasswordSalt = salt;
        account.MustChangePassword = false;
        await _dataService.SaveAsync(data);
    }

    public async Task EnsureStudentAccountAsync(string studentId, string initialPassword)
    {
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u =>
            u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
            u.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));

        if (account is not null)
            return;

        var (hash, salt) = PasswordHasher.Hash(initialPassword);
        data.Users.Add(new UserAccount
        {
            Username = studentId,
            StudentId = studentId,
            Role = "Student",
            PasswordHash = hash,
            PasswordSalt = salt,
            MustChangePassword = true
        });

        await _dataService.SaveAsync(data);
    }

    private async Task<UserAccount?> AuthenticateAsync(string username, string password, string role)
    {
        var data = await _dataService.LoadAsync();
        var account = data.Users.FirstOrDefault(u =>
            u.Role.Equals(role, StringComparison.OrdinalIgnoreCase) &&
            u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));

        return account is not null && PasswordHasher.Verify(password, account.PasswordHash, account.PasswordSalt)
            ? account
            : null;
    }
}
