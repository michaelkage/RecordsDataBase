using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class RankingService
{
    private readonly LocalDataService _dataService = new();

    public async Task<List<StudentRanking>> GetStudentRankingsAsync(string? classLevel = null)
    {
        var data = await _dataService.LoadAsync();
        var students = data.Students.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(classLevel)) students = students.Where(s => s.ClassLevel.Equals(classLevel, StringComparison.OrdinalIgnoreCase));

        return students.Select(student =>
        {
            var scores = data.Scores.Where(s => s.StudentId == student.Id).ToList();
            var total = scores.Sum(s => s.Total);
            var average = scores.Count == 0 ? 0 : total / scores.Count;
            return new StudentRanking(student.Id, student.Name, student.ClassLevel, scores.Count, Math.Round(average, 2));
        }).OrderByDescending(r => r.Average).ThenBy(r => r.Name).Select((r, i) => r with { Position = i + 1 }).ToList();
    }
}

public record StudentRanking(string StudentId, string Name, string ClassLevel, int SubjectsTaken, double Average, int Position = 0);
