using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class StudentPortalService
{
    private readonly LocalDataService _data = new();

    public async Task<StudentPortalSnapshot?> GetSnapshotAsync(string studentId)
    {
        var data = await _data.LoadAsync();
        var student = data.Students.FirstOrDefault(s => s.Id.Equals(studentId, StringComparison.OrdinalIgnoreCase));
        if (student is null) return null;

        var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id) ?? new StudentDetails { StudentId = student.Id };
        var rows = data.Scores
            .Where(s => s.StudentId.Equals(student.Id, StringComparison.OrdinalIgnoreCase))
            .Select(s =>
            {
                var subject = data.Subjects.FirstOrDefault(x => x.Id.Equals(s.SubjectId, StringComparison.OrdinalIgnoreCase));
                return new StudentScoreRow
                {
                    SubjectName = subject?.Name ?? "Unknown subject",
                    SubjectCode = subject?.Code ?? "",
                    ContinuousAssessment = s.ScoreValue,
                    Exam = s.ExamScore,
                    Total = s.Total,
                    Grade = s.Grade
                };
            }).OrderBy(x => x.SubjectName).ToList();

        var averages = data.Students.Select(s =>
        {
            var scores = data.Scores.Where(x => x.StudentId == s.Id).ToList();
            return new { Student = s, Average = scores.Count == 0 ? 0 : scores.Average(x => x.Total) };
        }).ToList();

        var classStudents = averages.Where(x => x.Student.ClassLevel.Equals(student.ClassLevel, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.Average).ToList();
        var departmentStudents = averages.Where(x =>
        {
            var d = data.StudentDetails.FirstOrDefault(detail => detail.StudentId == x.Student.Id);
            return x.Student.ClassLevel.Equals(student.ClassLevel, StringComparison.OrdinalIgnoreCase) &&
                   (d?.Department ?? "").Equals(details.Department, StringComparison.OrdinalIgnoreCase);
        }).OrderByDescending(x => x.Average).ToList();

        var classIndex = classStudents.FindIndex(x => x.Student.Id == student.Id);
        var departmentIndex = departmentStudents.FindIndex(x => x.Student.Id == student.Id);

        return new StudentPortalSnapshot
        {
            Student = student,
            Details = details,
            Scores = rows,
            Position = classIndex < 0 ? 0 : classIndex + 1,
            ClassSize = classStudents.Count,
            DepartmentPosition = departmentIndex < 0 ? 0 : departmentIndex + 1,
            DepartmentSize = departmentStudents.Count
        };
    }

    public async Task<List<Subject>> GetEnrolledSubjectsAsync(string studentId)
    {
        var data = await _data.LoadAsync();
        var ids = data.Scores.Where(x => x.StudentId == studentId).Select(x => x.SubjectId).ToHashSet();
        return data.Subjects.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Name).ToList();
    }
}
