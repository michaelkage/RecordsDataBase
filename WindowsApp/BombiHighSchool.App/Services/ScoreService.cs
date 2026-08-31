using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class ScoreService
{
    private readonly LocalDataService _dataService;

    public ScoreService(LocalDataService? dataService = null)
    {
        _dataService = dataService ?? new LocalDataService();
    }

    public async Task<List<Score>> GetAllAsync()
    {
        var data = await _dataService.LoadAsync();
        return data.Scores.ToList();
    }

    public async Task<List<Score>> GetForPeriodAsync(string session, string term)
    {
        if (string.IsNullOrWhiteSpace(session) || string.IsNullOrWhiteSpace(term))
            throw new ArgumentException("An academic session and term are required.");

        var data = await _dataService.LoadAsync();
        return data.Scores.Where(s =>
            s.Session.Equals(session, StringComparison.OrdinalIgnoreCase) &&
            s.Term.Equals(term, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<Score?> GetAsync(string studentId, string subjectId, string? session = null, string? term = null)
    {
        var data = await _dataService.LoadAsync();
        // An omitted period means the current period, never an arbitrary historical score.
        session = string.IsNullOrWhiteSpace(session) ? data.CurrentAcademicPeriod.Session : session;
        term = string.IsNullOrWhiteSpace(term) ? data.CurrentAcademicPeriod.Term : term;

        return data.Scores.FirstOrDefault(s =>
            s.StudentId == studentId &&
            s.SubjectId == subjectId &&
            s.Session.Equals(session, StringComparison.OrdinalIgnoreCase) &&
            s.Term.Equals(term, StringComparison.OrdinalIgnoreCase));
    }

    public async Task SaveAsync(string studentId, string subjectId, double test, double exam)
    {
        if (test is < 0 or > 40 || exam is < 0 or > 60)
            throw new ArgumentOutOfRangeException(nameof(test), "CA must be 0–40 and exam must be 0–60.");
        await SaveBulkAsync([new BulkScoreEntry(studentId, subjectId, test, exam)]);
    }

    public async Task SaveBulkAsync(IEnumerable<BulkScoreEntry> entries)
    {
        var pending = entries.ToList();
        if (pending.Count == 0) return;
        if (pending.Any(x => x.Test is < 0 or > 40 || x.Exam is < 0 or > 60))
            throw new ArgumentOutOfRangeException(nameof(entries), "CA must be 0–40 and exam must be 0–60.");

        await _dataService.UpdateAsync(data =>
        {
            var period = data.CurrentAcademicPeriod;
            foreach (var entry in pending)
            {
                if (!data.Students.Any(x => x.Id == entry.StudentId && !x.IsArchived))
                    throw new InvalidOperationException($"Student {entry.StudentId} does not exist or is archived.");
                if (!data.Subjects.Any(x => x.Id == entry.SubjectId))
                    throw new InvalidOperationException($"Subject {entry.SubjectId} does not exist.");
                if (!data.Enrollments.Any(x => x.StudentId == entry.StudentId && x.SubjectId == entry.SubjectId))
                    throw new InvalidOperationException("Every student must be enrolled in the subject before scores can be entered.");

                var score = data.Scores.FirstOrDefault(s =>
                    s.StudentId == entry.StudentId &&
                    s.SubjectId == entry.SubjectId &&
                    s.Session.Equals(period.Session, StringComparison.OrdinalIgnoreCase) &&
                    s.Term.Equals(period.Term, StringComparison.OrdinalIgnoreCase));

                if (score is null)
                {
                    score = new Score
                    {
                        Id = $"SCR{Guid.NewGuid():N}",
                        StudentId = entry.StudentId,
                        SubjectId = entry.SubjectId,
                        Session = period.Session,
                        Term = period.Term
                    };
                    data.Scores.Add(score);
                }

                score.ScoreValue = entry.Test;
                score.ExamScore = entry.Exam;
            }
            return Task.CompletedTask;
        });
    }

    public async Task DeleteAsync(string studentId, string subjectId, string? session = null, string? term = null)
    {
        await _dataService.UpdateAsync(data =>
        {
            session = string.IsNullOrWhiteSpace(session) ? data.CurrentAcademicPeriod.Session : session;
            term = string.IsNullOrWhiteSpace(term) ? data.CurrentAcademicPeriod.Term : term;

            data.Scores.RemoveAll(s =>
                s.StudentId == studentId &&
                s.SubjectId == subjectId &&
                s.Session.Equals(session, StringComparison.OrdinalIgnoreCase) &&
                s.Term.Equals(term, StringComparison.OrdinalIgnoreCase));
            return Task.CompletedTask;
        });
    }
}

public sealed record BulkScoreEntry(string StudentId, string SubjectId, double Test, double Exam);
