using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class GlobalSearchService
{
    private readonly LocalDataService _dataService = new();

    public async Task<GlobalSearchResults> SearchAsync(string query)
    {
        var q = query.Trim();
        if (string.IsNullOrWhiteSpace(q)) return new([], [], []);

        var data = await _dataService.LoadAsync();
        var students = data.Students
            .Where(s => !s.IsArchived && (s.Id.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || s.ClassLevel.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .Take(20)
            .ToList();

        var subjects = data.Subjects
            .Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Id.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Take(20)
            .ToList();

        var details = data.StudentDetails
            .Where(d => d.AdmissionNumber.Contains(q, StringComparison.OrdinalIgnoreCase) || d.Arm.Contains(q, StringComparison.OrdinalIgnoreCase) || d.Department.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(d => d.StudentId)
            .ToHashSet();
        foreach (var student in data.Students.Where(s => details.Contains(s.Id) && !students.Any(x => x.Id == s.Id) && !s.IsArchived).Take(20)) students.Add(student);

        return new GlobalSearchResults(students, subjects, ["Add student", "Enter scores", "Create backup", "Open settings"]);
    }
}

public sealed record GlobalSearchResults(IReadOnlyList<Student> Students, IReadOnlyList<Subject> Subjects, IReadOnlyList<string> Actions);
