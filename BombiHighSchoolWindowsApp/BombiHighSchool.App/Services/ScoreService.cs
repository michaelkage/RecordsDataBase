using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class ScoreService
{
    private readonly LocalDataService _dataService = new();

    public async Task<List<Score>> GetAllAsync() { var data = await _dataService.LoadAsync(); return data.Scores.ToList(); }

    public async Task<List<Score>> GetForPeriodAsync(string session, string term)
    {
        var data = await _dataService.LoadAsync();
        return data.Scores.Where(s => s.Session.Equals(session, StringComparison.OrdinalIgnoreCase) && s.Term.Equals(term, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<Score?> GetAsync(string studentId, string subjectId, string? session = null, string? term = null)
    {
        var data = await _dataService.LoadAsync();
        var query = data.Scores.Where(s => s.StudentId == studentId && s.SubjectId == subjectId);
        if (!string.IsNullOrWhiteSpace(session)) query = query.Where(s => s.Session.Equals(session, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(term)) query = query.Where(s => s.Term.Equals(term, StringComparison.OrdinalIgnoreCase));
        return query.FirstOrDefault();
    }

    public async Task SaveAsync(string studentId, string subjectId, double test, double exam)
    {
        if (test is < 0 or > 40 || exam is < 0 or > 60) throw new ArgumentOutOfRangeException(nameof(test), "CA must be 0–40 and exam must be 0–60.");
        await SaveBulkAsync([new BulkScoreEntry(studentId, subjectId, test, exam)]);
    }

    public async Task SaveBulkAsync(IEnumerable<BulkScoreEntry> entries)
    {
        var pending = entries.ToList();
        if (pending.Count == 0) return;
        if (pending.Any(x => x.Test is < 0 or > 40 || x.Exam is < 0 or > 60)) throw new ArgumentOutOfRangeException(nameof(entries), "CA must be 0–40 and exam must be 0–60.");

        await _dataService.UpdateAsync(data =>
        {
            var period = data.CurrentAcademicPeriod;
            foreach (var entry in pending)
            {
                if (!data.Students.Any(x => x.Id == entry.StudentId && !x.IsArchived)) throw new InvalidOperationException($"Student {entry.StudentId} does not exist or is archived.");
                if (!data.Subjects.Any(x => x.Id == entry.SubjectId)) throw new InvalidOperationException($"Subject {entry.SubjectId} does not exist.");
                if (!data.Enrollments.Any(x => x.StudentId == entry.StudentId && x.SubjectId == entry.SubjectId)) throw new InvalidOperationException("Every student must be enrolled in the subject before scores can be entered.");

                var score = data.Scores.FirstOrDefault(s => s.StudentId == entry.StudentId && s.SubjectId == entry.SubjectId && s.Session == period.Session && s.Term == period.Term);
                if (score is null)
                {
                    score = new Score { Id = $"SCR{Guid.NewGuid():N}", StudentId = entry.StudentId, SubjectId = entry.SubjectId, Session = period.Session, Term = period.Term };
                    data.Scores.Add(score);
                }
                score.ScoreValue = entry.Test;
                score.ExamScore = entry.Exam;
            }
            return Task.CompletedTask;
        });
    }

    public async Task DeleteAsync(string studentId, string subjectId) => await _dataService.UpdateAsync(data => { data.Scores.RemoveAll(s => s.StudentId == studentId && s.SubjectId == subjectId); return Task.CompletedTask; });
}

public sealed record BulkScoreEntry(string StudentId, string SubjectId, double Test, double Exam);
