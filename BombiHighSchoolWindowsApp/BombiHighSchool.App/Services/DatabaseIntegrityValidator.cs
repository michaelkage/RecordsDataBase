using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public static class DatabaseIntegrityValidator
{
    public static void Validate(SchoolData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        EnsureRequiredUnique(data.Students.Select(x => x.Id), "student ID");
        EnsureRequiredUnique(data.StudentDetails.Select(x => x.StudentId), "student details student ID");
        EnsureRequiredUnique(data.Subjects.Select(x => x.Id), "subject ID");
        EnsureRequiredUnique(data.Users.Select(x => x.Username), "username");

        var studentIds = data.Students
            .Select(x => x.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var subjectIds = data.Subjects
            .Select(x => x.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var details in data.StudentDetails)
            Require(studentIds.Contains(details.StudentId),
                $"StudentDetails references missing student '{details.StudentId}'.");

        var enrollmentKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var enrollment in data.Enrollments)
        {
            Require(studentIds.Contains(enrollment.StudentId),
                $"Enrollment references missing student '{enrollment.StudentId}'.");
            Require(subjectIds.Contains(enrollment.SubjectId),
                $"Enrollment references missing subject '{enrollment.SubjectId}'.");

            var key = $"{enrollment.StudentId}\u001f{enrollment.SubjectId}";
            Require(enrollmentKeys.Add(key),
                $"Duplicate enrollment for student '{enrollment.StudentId}' and subject '{enrollment.SubjectId}'.");
        }

        var scoreKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var score in data.Scores)
        {
            Require(!string.IsNullOrWhiteSpace(score.Id), "A score has no ID.");
            Require(studentIds.Contains(score.StudentId),
                $"Score references missing student '{score.StudentId}'.");
            Require(subjectIds.Contains(score.SubjectId),
                $"Score references missing subject '{score.SubjectId}'.");
            Require(!string.IsNullOrWhiteSpace(score.Session),
                $"Score '{score.Id}' has no academic session.");
            Require(!string.IsNullOrWhiteSpace(score.Term),
                $"Score '{score.Id}' has no academic term.");
            Require(double.IsFinite(score.ScoreValue) && score.ScoreValue is >= 0 and <= 40,
                $"Score '{score.Id}' has an invalid test score. Test must be 0–40.");
            Require(double.IsFinite(score.ExamScore) && score.ExamScore is >= 0 and <= 60,
                $"Score '{score.Id}' has an invalid exam score. Exam must be 0–60.");

            var key = $"{score.StudentId}\u001f{score.SubjectId}\u001f{score.Session}\u001f{score.Term}";
            Require(scoreKeys.Add(key),
                $"Duplicate score for student '{score.StudentId}', subject '{score.SubjectId}', session '{score.Session}', term '{score.Term}'.");
        }

        foreach (var account in data.Users.Where(x =>
                     x.Role.Equals("Student", StringComparison.OrdinalIgnoreCase)))
        {
            Require(!string.IsNullOrWhiteSpace(account.StudentId),
                $"Student account '{account.Username}' has no student ID.");
            Require(studentIds.Contains(account.StudentId),
                $"Student account '{account.Username}' references missing student '{account.StudentId}'.");
        }

        Require(data.NextStudentNumber >= 0, "Next student number cannot be negative.");
        Require(data.NextAdmissionNumber >= 0, "Next admission number cannot be negative.");
    }

    private static void EnsureRequiredUnique(IEnumerable<string> values, string label)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values)
        {
            Require(!string.IsNullOrWhiteSpace(value), $"A {label} is missing.");
            Require(seen.Add(value), $"Duplicate {label}: '{value}'.");
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new DatabaseIntegrityException(message);
    }
}

public sealed class DatabaseIntegrityException : Exception
{
    public DatabaseIntegrityException(string message) : base(message) { }
}
