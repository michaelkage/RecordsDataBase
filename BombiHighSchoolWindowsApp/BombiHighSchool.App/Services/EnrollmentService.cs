using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class EnrollmentService
{
    private readonly LocalDataService _data = new();
    public async Task<List<Subject>> GetForStudentAsync(string studentId) { var data = await _data.LoadAsync(); var ids = data.Enrollments.Where(x => x.StudentId == studentId).Select(x => x.SubjectId).ToHashSet(); return data.Subjects.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Name).ToList(); }

    public async Task AdminEnrollAsync(string studentId, string subjectId)
    {
        await _data.UpdateAsync(data =>
        {
            if (!data.Students.Any(x => x.Id == studentId) || !data.Subjects.Any(x => x.Id == subjectId)) throw new InvalidOperationException("Student or subject does not exist.");
            if (!data.Enrollments.Any(x => x.StudentId == studentId && x.SubjectId == subjectId)) data.Enrollments.Add(new SubjectEnrollment { StudentId = studentId, SubjectId = subjectId });
            return Task.CompletedTask;
        });
    }

    public async Task AdminUnenrollAsync(string studentId, string subjectId)
    {
        await _data.UpdateAsync(data =>
        {
            var subject = data.Subjects.FirstOrDefault(x => x.Id == subjectId);
            if (subject?.IsCompulsory == true) throw new InvalidOperationException("Compulsory subjects cannot be removed.");
            data.Enrollments.RemoveAll(x => x.StudentId == studentId && x.SubjectId == subjectId);
            data.Scores.RemoveAll(x => x.StudentId == studentId && x.SubjectId == subjectId);
            return Task.CompletedTask;
        });
    }

    public async Task EnsureCompulsorySubjectsAsync(string studentId)
    {
        await _data.UpdateAsync(data =>
        {
            if (!data.Students.Any(x => x.Id == studentId)) return Task.CompletedTask;
            foreach (var subject in data.Subjects.Where(x => x.IsCompulsory && !data.Enrollments.Any(e => e.StudentId == studentId && e.SubjectId == x.Id))) data.Enrollments.Add(new SubjectEnrollment { StudentId = studentId, SubjectId = subject.Id });
            return Task.CompletedTask;
        });
    }
}
