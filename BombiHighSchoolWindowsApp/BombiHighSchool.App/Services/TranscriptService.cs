using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class TranscriptService
{
    private readonly LocalDataService _data = new();

    public async Task<(Student Student, StudentDetails Details, List<TranscriptRow> Rows)?> BuildAsync(string studentId)
    {
        var data = await _data.LoadAsync();
        var student = data.Students.FirstOrDefault(s => s.Id == studentId);
        if (student is null) return null;
        var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == studentId) ?? new StudentDetails { StudentId = studentId };
        var rows = data.Scores.Where(s => s.StudentId == studentId).Select(s =>
        {
            var subject = data.Subjects.FirstOrDefault(x => x.Id == s.SubjectId);
            return new TranscriptRow { Subject = subject?.Name ?? "Unknown subject", Code = subject?.Code ?? "", CA = s.ScoreValue, Exam = s.ExamScore, Total = s.Total, Grade = s.Grade };
        }).OrderBy(x => x.Subject).ToList();
        return (student, details, rows);
    }
}
