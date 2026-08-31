namespace BombiHighSchool.App.Services;

public static class SessionService
{
    public static string? StudentId { get; private set; }
    public static string? Username { get; private set; }
    public static string? Role { get; private set; }

    public static bool IsStudentSession => Role?.Equals("Student", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrWhiteSpace(StudentId);
    public static bool IsAdminSession => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrWhiteSpace(Username);

    public static void StartStudent(string studentId)
    {
        Clear();
        StudentId = studentId.Trim();
        Username = StudentId;
        Role = "Student";
    }

    public static void StartAdmin(string username)
    {
        Clear();
        Username = username.Trim();
        Role = "Admin";
    }

    public static void Clear()
    {
        StudentId = null;
        Username = null;
        Role = null;
    }
}
