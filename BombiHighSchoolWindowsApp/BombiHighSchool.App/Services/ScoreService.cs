using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class ScoreService
{
    private readonly LocalDataService _dataService = new();
    public async Task<List<Score>> GetAllAsync() { var data = await _dataService.LoadAsync(); return data.Scores.ToList(); }
    public async Task<Score?> GetAsync(string studentId, string subjectId) { var data = await _dataService.LoadAsync(); return data.Scores.FirstOrDefault(s => s.StudentId == studentId && s.SubjectId == subjectId); }

    public async Task SaveAsync(string studentId, string subjectId, double test, double exam)
    {
        if (test is < 0 or > 40 || exam is < 0 or > 60) throw new ArgumentOutOfRangeException(nameof(test), "CA must be 0–40 and exam must be 0–60.");
        await _dataService.UpdateAsync(data =>
        {
            if (!data.Students.Any(x => x.Id == studentId)) throw new InvalidOperationException("Student does not exist.");
            if (!data.Subjects.Any(x => x.Id == subjectId)) throw new InvalidOperationException("Subject does not exist.");
            if (!data.Enrollments.Any(x => x.StudentId == studentId && x.SubjectId == subjectId)) throw new InvalidOperationException("The subject must be enrolled before entering a score.");
            var score = data.Scores.FirstOrDefault(s => s.StudentId == studentId && s.SubjectId == subjectId);
            if (score is null) { score = new Score { Id = $"SCR{Guid.NewGuid():N}", StudentId = studentId, SubjectId = subjectId }; data.Scores.Add(score); }
            score.ScoreValue = test; score.ExamScore = exam;
            return Task.CompletedTask;
        });
    }

    public async Task DeleteAsync(string studentId, string subjectId) => await _dataService.UpdateAsync(data => { data.Scores.RemoveAll(s => s.StudentId == studentId && s.SubjectId == subjectId); return Task.CompletedTask; });
}
