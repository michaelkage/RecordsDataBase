using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class StudentService
{
    private readonly LocalDataService _dataService;
    private readonly AuthenticationService _authenticationService;

    public StudentService(LocalDataService? dataService = null)
    {
        _dataService = dataService ?? new LocalDataService();
        _authenticationService = new AuthenticationService(_dataService);
    }

    public async Task<List<Student>> GetAllAsync()
    {
        var data = await _dataService.LoadAsync();
        return data.Students.ToList();
    }

    public async Task<Student> AddAsync(string name, int age, string gender, string classLevel, string arm, string department)
    {
        var data = await _dataService.LoadAsync();
        var student = new Student { Id = GenerateNextId(data.Students), Name = name.Trim(), Age = age, Gender = gender.Trim(), ClassLevel = classLevel.Trim() };
        data.Students.Add(student);
        data.StudentDetails.Add(new StudentDetails { StudentId = student.Id, Arm = arm.Trim(), Department = department.Trim(), Status = "Active" });
        await _dataService.SaveAsync(data);
        await _authenticationService.EnsureStudentAccountAsync(student.Id, "Welcome123!");
        return student;
    }

    public async Task UpdateAsync(Student student)
    {
        var data = await _dataService.LoadAsync();
        var existing = data.Students.FirstOrDefault(s => s.Id == student.Id);
        if (existing is null) throw new InvalidOperationException("The student could not be found.");
        existing.Name = student.Name.Trim(); existing.Age = student.Age; existing.Gender = student.Gender.Trim(); existing.ClassLevel = student.ClassLevel.Trim();
        await _dataService.SaveAsync(data);
    }

    public async Task DeleteAsync(string id)
    {
        var data = await _dataService.LoadAsync();
        var student = data.Students.FirstOrDefault(s => s.Id == id);
        if (student is null) return;
        data.Students.Remove(student);
        data.StudentDetails.RemoveAll(d => d.StudentId == id);
        data.Scores.RemoveAll(s => s.StudentId == id);
        data.Users.RemoveAll(u => u.StudentId == id && u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase));
        await _dataService.SaveAsync(data);
    }

    private static string GenerateNextId(IEnumerable<Student> students)
    {
        var nextNumber = students.Select(s => s.Id).Where(id => id.StartsWith("BHS", StringComparison.OrdinalIgnoreCase)).Select(id => id[3..]).Select(value => int.TryParse(value, out var number) ? number : 0).DefaultIfEmpty(0).Max() + 1;
        return $"BHS{nextNumber:000000}";
    }
}
