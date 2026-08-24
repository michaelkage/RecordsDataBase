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

    public async Task<List<Student>> GetAllAsync(bool includeArchived = false)
    {
        var data = await _dataService.LoadAsync();
        return data.Students.Where(s => includeArchived || !IsArchived(data, s.Id)).ToList();
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

    public async Task ArchiveAsync(string id)
    {
        var data = await _dataService.LoadAsync();
        var student = data.Students.FirstOrDefault(s => s.Id == id) ?? throw new InvalidOperationException("The student could not be found.");
        var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == id);
        if (details is not null) details.Status = "Archived";
        var account = data.Users.FirstOrDefault(u => u.StudentId == id && u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase));
        if (account is not null) account.IsEnabled = false;
        await _dataService.SaveAsync(data);
    }

    public async Task RestoreAsync(string id)
    {
        var data = await _dataService.LoadAsync();
        var student = data.Students.FirstOrDefault(s => s.Id == id) ?? throw new InvalidOperationException("The student could not be found.");
        var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == id);
        if (details is not null) details.Status = "Active";
        var account = data.Users.FirstOrDefault(u => u.StudentId == id && u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase));
        if (account is not null) account.IsEnabled = true;
        await _dataService.SaveAsync(data);
    }

    public Task ResetStudentPasswordAsync(string id, string newPassword) => _authenticationService.SetStudentPasswordAsync(id, newPassword);
    public Task SetStudentAccountEnabledAsync(string id, bool enabled) => _authenticationService.SetStudentEnabledAsync(id, enabled);

    private static bool IsArchived(SchoolData data, string id) => data.StudentDetails.FirstOrDefault(d => d.StudentId == id)?.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase) == true;
    private static string GenerateNextId(IEnumerable<Student> students)
    {
        var nextNumber = students.Select(s => s.Id).Where(id => id.StartsWith("BHS", StringComparison.OrdinalIgnoreCase)).Select(id => id[3..]).Select(value => int.TryParse(value, out var number) ? number : 0).DefaultIfEmpty(0).Max() + 1;
        return $"BHS{nextNumber:000000}";
    }
}
