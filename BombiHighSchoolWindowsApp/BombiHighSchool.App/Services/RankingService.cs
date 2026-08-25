using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class RankingService
{
    private readonly LocalDataService _dataService = new();

    public async Task<List<StudentRanking>> GetStudentRankingsAsync(string? classLevel = null, string? arm = null, string? session = null, string? term = null)
    {
        var data = await _dataService.LoadAsync();
        var period = data.CurrentAcademicPeriod;
        session ??= period.Session;
        term ??= period.Term;

        var students = data.Students.Where(s => !s.IsArchived).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(classLevel)) students = students.Where(s => s.ClassLevel.Equals(classLevel, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(arm))
        {
            var ids = data.StudentDetails.Where(d => d.Arm.Equals(arm, StringComparison.OrdinalIgnoreCase)).Select(d => d.StudentId).ToHashSet();
            students = students.Where(s => ids.Contains(s.Id));
        }

        var scores = data.Scores.Where(s => s.Session.Equals(session, StringComparison.OrdinalIgnoreCase) && s.Term.Equals(term, StringComparison.OrdinalIgnoreCase)).ToList();
        return students.Select(student =>
        {
            var studentScores = scores.Where(s => s.StudentId == student.Id).ToList();
            var total = studentScores.Sum(s => s.Total);
            var average = studentScores.Count == 0 ? 0 : total / studentScores.Count;
            var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id);
            return new StudentRanking(student.Id, student.Name, student.ClassLevel, details?.Arm ?? "", studentScores.Count, Math.Round(average, 2));
        }).Where(r => r.SubjectsTaken > 0).OrderByDescending(r => r.Average).ThenBy(r => r.Name).Select((r, i) => r with { Position = i + 1 }).ToList();
    }
}

public record StudentRanking(string StudentId, string Name, string ClassLevel, string Arm, int SubjectsTaken, double Average, int Position = 0);
