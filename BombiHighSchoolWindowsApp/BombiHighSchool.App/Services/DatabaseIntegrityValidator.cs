using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public static class DatabaseIntegrityValidator
{
    public static void Validate(SchoolData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Unique(data.Students.Select(x => x.Id), "student ID");
        Unique(data.StudentDetails.Select(x => x.StudentId), "student details student ID");
        Unique(data.Subjects.Select(x => x.Id), "subject ID");
        Unique(data.Users.Select(x => x.Username), "username");

        var students = data.Students.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var subjects = data.Subjects.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var detailIds = data.StudentDetails.Select(x => x.StudentId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var admissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var d in data.StudentDetails)
        {
            Require(students.ContainsKey(d.StudentId), $"StudentDetails references missing student '{d.StudentId}'.");
            if (!string.IsNullOrWhiteSpace(d.AdmissionNumber)) Require(admissions.Add(d.AdmissionNumber), $"Duplicate admission number '{d.AdmissionNumber}'.");
        }
        foreach (var s in data.Students) Require(detailIds.Contains(s.Id), $"Student '{s.Id}' has no StudentDetails record.");

        var enrollments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in data.Enrollments)
        {
            Require(students.TryGetValue(e.StudentId, out var student), $"Enrollment references missing student '{e.StudentId}'.");
            Require(subjects.ContainsKey(e.SubjectId), $"Enrollment references missing subject '{e.SubjectId}'.");
            Require(enrollments.Add($"{e.StudentId}\u001f{e.SubjectId}"), $"Duplicate enrollment for student '{e.StudentId}' and subject '{e.SubjectId}'.");
            Require(!student!.IsArchived, $"Archived student '{e.StudentId}' cannot have an active enrollment.");
        }

        var scores = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var score in data.Scores)
        {
            Require(!string.IsNullOrWhiteSpace(score.Id), "A score has no ID.");
            Require(students.ContainsKey(score.StudentId), $"Score references missing student '{score.StudentId}'.");
            Require(subjects.ContainsKey(score.SubjectId), $"Score references missing subject '{score.SubjectId}'.");
            Require(!string.IsNullOrWhiteSpace(score.Session), $"Score '{score.Id}' has no academic session.");
            Require(!string.IsNullOrWhiteSpace(score.Term), $"Score '{score.Id}' has no academic term.");
            Require(double.IsFinite(score.ScoreValue) && score.ScoreValue is >= 0 and <= 40, $"Score '{score.Id}' has an invalid test score.");
            Require(double.IsFinite(score.ExamScore) && score.ExamScore is >= 0 and <= 60, $"Score '{score.Id}' has an invalid exam score.");
            Require(scores.Add($"{score.StudentId}\u001f{score.SubjectId}\u001f{score.Session}\u001f{score.Term}"), $"Duplicate score for student '{score.StudentId}', subject '{score.SubjectId}', session '{score.Session}', term '{score.Term}'.");
        }

        foreach (var account in data.Users.Where(x => x.Role.Equals("Student", StringComparison.OrdinalIgnoreCase)))
        {
            Require(!string.IsNullOrWhiteSpace(account.StudentId), $"Student account '{account.Username}' has no student ID.");
            Require(students.ContainsKey(account.StudentId), $"Student account '{account.Username}' references a missing student.");
            Require(account.Username.Equals(account.StudentId, StringComparison.OrdinalIgnoreCase), $"Student account '{account.Username}' must use its student ID as username.");
        }
        foreach (var student in data.Students)
        {
            var count = data.Users.Count(x => x.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) && x.StudentId.Equals(student.Id, StringComparison.OrdinalIgnoreCase));
            Require(count == 1, $"Student '{student.Id}' must have exactly one student account.");
        }
        Require(data.NextStudentNumber >= 0, "Next student number cannot be negative.");
        Require(data.NextAdmissionNumber >= 0, "Next admission number cannot be negative.");
    }

    private static void Unique(IEnumerable<string> values, string label)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values) { Require(!string.IsNullOrWhiteSpace(value), $"A {label} is missing."); Require(seen.Add(value), $"Duplicate {label}: '{value}'."); }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new DatabaseIntegrityException(message); }
}

public sealed class DatabaseIntegrityException : Exception
{
    public DatabaseIntegrityException(string message) : base(message) { }
}
