using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class ScoreService
{
    private readonly LocalDataService _dataService = new();

    public async Task<List<Score>> GetAllAsync()
    {
        var data = await _dataService.LoadAsync();
        return data.Scores.ToList();
    }

    public async Task SaveAsync(string studentId, string subjectId, double test, double exam)
    {
        var data = await _dataService.LoadAsync();
        var score = data.Scores.FirstOrDefault(s => s.StudentId == studentId && s.SubjectId == subjectId);
        if (score is null)
        {
            score = new Score { Id = $"SCR{Guid.NewGuid():N}", StudentId = studentId, SubjectId = subjectId };
            data.Scores.Add(score);
        }
        score.ScoreValue = Math.Clamp(test, 0, 40);
        score.ExamScore = Math.Clamp(exam, 0, 60);
        await _dataService.SaveAsync(data);
    }

    public async Task DeleteAsync(string studentId, string subjectId)
    {
        var data = await _dataService.LoadAsync();
        data.Scores.RemoveAll(s => s.StudentId == studentId && s.SubjectId == subjectId);
        await _dataService.SaveAsync(data);
    }
}
