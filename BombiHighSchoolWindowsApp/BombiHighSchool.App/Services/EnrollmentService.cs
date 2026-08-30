using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class EnrollmentService
{
    private readonly LocalDataService _data;
    public EnrollmentService(LocalDataService? dataService = null) => _data = dataService ?? LocalDataService.Shared;

    public async Task<List<Subject>> GetForStudentAsync(string studentId)
    {
        var data = await _data.LoadAsync();
        var student = data.Students.FirstOrDefault(x => x.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
        if (student is null || student.IsArchived) return [];
        var ids = data.Enrollments.Where(x => x.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase)).Select(x => x.SubjectId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return data.Subjects.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Name).ToList();
    }

    public async Task AdminEnrollAsync(string studentId, string subjectId)
    {
        await _data.UpdateAsync(data =>
        {
            var student = data.Students.FirstOrDefault(x => x.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
            var subject = data.Subjects.FirstOrDefault(x => x.Id.Equals(subjectId, StringComparison.OrdinalIgnoreCase));
            if (student is null || student.IsArchived) throw new InvalidOperationException("Student does not exist or is archived.");
            if (subject is null) throw new InvalidOperationException("Subject does not exist.");
            if (!data.Enrollments.Any(x => x.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase) && x.SubjectId.Equals(subjectId, StringComparison.OrdinalIgnoreCase)))
                data.Enrollments.Add(new SubjectEnrollment { StudentId = studentId, SubjectId = subjectId });
            return Task.CompletedTask;
        });
    }

    public async Task AdminUnenrollAsync(string studentId, string subjectId)
    {
        await _data.UpdateAsync(data =>
        {
            var student = data.Students.FirstOrDefault(x => x.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
            var subject = data.Subjects.FirstOrDefault(x => x.Id.Equals(subjectId, StringComparison.OrdinalIgnoreCase));
            if (student is null) throw new InvalidOperationException("Student does not exist.");
            if (subject is null) throw new InvalidOperationException("Subject does not exist.");
            if (subject.IsCompulsory) throw new InvalidOperationException("Compulsory subjects cannot be removed.");
            // Enrollment is current state; historical scores remain available for report history.
            data.Enrollments.RemoveAll(x => x.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase) && x.SubjectId.Equals(subjectId, StringComparison.OrdinalIgnoreCase));
            return Task.CompletedTask;
        });
    }

    public async Task EnsureCompulsorySubjectsAsync(string studentId)
    {
        await _data.UpdateAsync(data =>
        {
            var student = data.Students.FirstOrDefault(x => x.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
            if (student is null || student.IsArchived) return Task.CompletedTask;
            foreach (var subject in data.Subjects.Where(x => x.IsCompulsory && !data.Enrollments.Any(e => e.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase) && e.SubjectId.Equals(x.Id, StringComparison.OrdinalIgnoreCase))))
                data.Enrollments.Add(new SubjectEnrollment { StudentId = studentId, SubjectId = subject.Id });
            return Task.CompletedTask;
        });
    }
}
