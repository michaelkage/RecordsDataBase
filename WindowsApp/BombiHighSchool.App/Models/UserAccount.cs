namespace BombiHighSchool.App.Models;

public class UserAccount
{
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string PasswordSalt { get; set; } = "";
    public bool MustChangePassword { get; set; }
    public bool IsEnabled { get; set; } = true;
}
