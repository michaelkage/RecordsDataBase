using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class SubjectService
{
    private readonly LocalDataService _dataService = new();

    public async Task<List<Subject>> GetAllAsync()
    {
        var data = await _dataService.LoadAsync();
        return data.Subjects.OrderBy(s => s.Name).ToList();
    }

    public async Task<Subject> AddAsync(string name, string code, string department, bool compulsory)
    {
        var data = await _dataService.LoadAsync();
        var subject = new Subject
        {
            Id = GenerateNextId(data.Subjects), Name = name.Trim(), Code = code.Trim().ToUpperInvariant(),
            Department = department.Trim(), IsCompulsory = compulsory
        };
        data.Subjects.Add(subject);
        await _dataService.SaveAsync(data);
        return subject;
    }

    public async Task UpdateAsync(Subject subject)
    {
        var data = await _dataService.LoadAsync();
        var existing = data.Subjects.FirstOrDefault(s => s.Id == subject.Id) ?? throw new InvalidOperationException("Subject not found.");
        existing.Name = subject.Name.Trim(); existing.Code = subject.Code.Trim().ToUpperInvariant();
        existing.Department = subject.Department.Trim(); existing.IsCompulsory = subject.IsCompulsory;
        await _dataService.SaveAsync(data);
    }

    public async Task DeleteAsync(string id)
    {
        var data = await _dataService.LoadAsync();
        data.Subjects.RemoveAll(s => s.Id == id);
        data.Scores.RemoveAll(s => s.SubjectId == id);
        await _dataService.SaveAsync(data);
    }

    private static string GenerateNextId(IEnumerable<Subject> subjects)
    {
        var next = subjects.Select(s => s.Id.StartsWith("SUB", StringComparison.OrdinalIgnoreCase) && int.TryParse(s.Id[3..], out var n) ? n : 0).DefaultIfEmpty(0).Max() + 1;
        return $"SUB{next:0000}";
    }
}
