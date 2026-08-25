using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public static class DatabaseIntegrityValidator
{
    public static void Validate(SchoolData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        EnsureUnique(data.Students.Select(x => x.Id), "student ID");
        EnsureUnique(data.StudentDetails.Select(x => x.StudentId), "student details student ID");
        EnsureUnique(data.Subjects.Select(x => x.Id), "subject ID");
        EnsureUnique(data.Users.Select(x => x.Username), "username");

        var studentIds = data.Students.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var subjectIds = data.Subjects.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var details in data.StudentDetails)
            Require(studentIds.Contains(details.StudentId), $"StudentDetails references missing student '{details.StudentId}'.");
        foreach (var enrollment in data.Enrollments)
        {
            Require(studentIds.Contains(enrollment.StudentId), $"Enrollment references missing student '{enrollment.StudentId}'.");
            Require(subjectIds.Contains(enrollment.SubjectId), $"Enrollment references missing subject '{enrollment.SubjectId}'.");
        }
        foreach (var score in data.Scores)
        {
            Require(studentIds.Contains(score.StudentId), $"Score references missing student '{score.StudentId}'.");
            Require(subjectIds.Contains(score.SubjectId), $"Score references missing subject '{score.SubjectId}'.");
            Require(!string.IsNullOrWhiteSpace(score.Session), $"Score '{score.Id}' has no academic session.");
            Require(!string.IsNullOrWhiteSpace(score.Term), $"Score '{score.Id}' has no academic term.");
        }
        foreach (var account in data.Users.Where(x => x.Role.Equals("Student", StringComparison.OrdinalIgnoreCase)))
            Require(studentIds.Contains(account.StudentId), $"Student account '{account.Username}' references missing student '{account.StudentId}'.");
    }

    private static void EnsureUnique(IEnumerable<string> values, string label)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            if (!seen.Add(value)) throw new DatabaseIntegrityException($"Duplicate {label}: '{value}'.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new DatabaseIntegrityException(message);
    }
}

public sealed class DatabaseIntegrityException : Exception
{
    public DatabaseIntegrityException(string message) : base(message) { }
}
