namespace BombiHighSchool.App.Services;

public static class SessionService
{
    public static string? StudentId { get; set; }
    public static string? Username { get; set; }
    public static string? Role { get; set; }

    public static void StartStudent(string studentId)
    {
        StudentId = studentId;
        Username = studentId;
        Role = "Student";
    }

    public static void StartAdmin(string username)
    {
        StudentId = null;
        Username = username;
        Role = "Admin";
    }

    public static void Clear()
    {
        StudentId = null;
        Username = null;
        Role = null;
    }
}
