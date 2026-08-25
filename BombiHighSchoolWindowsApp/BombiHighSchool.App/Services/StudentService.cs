using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class StudentService
{
    private readonly LocalDataService _dataService;
    private readonly AuthenticationService _authenticationService;
    public StudentService(LocalDataService? dataService = null) { _dataService = dataService ?? new LocalDataService(); _authenticationService = new AuthenticationService(_dataService); }

    public async Task<List<Student>> GetAllAsync(bool includeArchived = false)
    {
        var data = await _dataService.LoadAsync();
        return data.Students.Where(s => includeArchived || (!s.IsArchived && !IsArchived(data, s.Id))).ToList();
    }

    public async Task<Student> AddAsync(string name, string gender, string classLevel, string arm, string department)
    {
        Student student = new();
        await _dataService.UpdateAsync(data =>
        {
            student = new Student { Id = GenerateNextId(data.Students), Name = name.Trim(), Age = 0, Gender = gender.Trim(), ClassLevel = classLevel.Trim(), IsArchived = false };
            data.Students.Add(student);
            data.StudentDetails.Add(new StudentDetails { StudentId = student.Id, Arm = arm.Trim(), Department = department.Trim(), AdmissionNumber = GenerateNextAdmissionNumber(data.StudentDetails), Status = "Active" });
            return Task.CompletedTask;
        });
        await _authenticationService.EnsureStudentAccountAsync(student.Id, "Welcome123!");
        return student;
    }

    public async Task UpdateProfileAsync(Student student, StudentDetails details)
    {
        await _dataService.UpdateAsync(data =>
        {
            var existing = data.Students.FirstOrDefault(s => s.Id == student.Id) ?? throw new InvalidOperationException("The student could not be found.");
            existing.Name = student.Name.Trim();
            existing.Age = student.Age;
            existing.Gender = student.Gender.Trim();
            existing.ClassLevel = student.ClassLevel.Trim();
            existing.IsArchived = false;

            var existingDetails = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id);
            if (existingDetails is null)
            {
                details.StudentId = student.Id;
                data.StudentDetails.Add(details);
            }
            else
            {
                existingDetails.Arm = details.Arm.Trim();
                existingDetails.Department = details.Department.Trim();
                existingDetails.DateOfBirth = details.DateOfBirth.Trim();
                if (!string.IsNullOrWhiteSpace(details.AdmissionNumber)) existingDetails.AdmissionNumber = details.AdmissionNumber.Trim();
                existingDetails.ParentName = details.ParentName.Trim();
                existingDetails.ParentPhone = details.ParentPhone.Trim();
                existingDetails.Address = details.Address.Trim();
                existingDetails.Email = details.Email.Trim();
                existingDetails.Status = string.IsNullOrWhiteSpace(details.Status) ? "Active" : details.Status.Trim();
            }
            return Task.CompletedTask;
        });
    }

    public async Task UpdateAsync(Student student)
    {
        var data = await _dataService.LoadAsync();
        var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id) ?? new StudentDetails { StudentId = student.Id };
        await UpdateProfileAsync(student, details);
    }

    public async Task ArchiveAsync(string id)
    {
        await _dataService.UpdateAsync(data =>
        {
            var student = data.Students.FirstOrDefault(s => s.Id == id) ?? throw new InvalidOperationException("The student could not be found.");
            student.IsArchived = true;
            var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == id);
            if (details is not null) details.Status = "Archived";
            var account = data.Users.FirstOrDefault(u => u.StudentId == id && u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase));
            if (account is not null) account.IsEnabled = false;
            return Task.CompletedTask;
        });
    }

    public async Task RestoreAsync(string id)
    {
        await _dataService.UpdateAsync(data =>
        {
            var student = data.Students.FirstOrDefault(s => s.Id == id) ?? throw new InvalidOperationException("The student could not be found.");
            student.IsArchived = false;
            var details = data.StudentDetails.FirstOrDefault(d => d.StudentId == id);
            if (details is not null) details.Status = "Active";
            var account = data.Users.FirstOrDefault(u => u.StudentId == id && u.Role.Equals("Student", StringComparison.OrdinalIgnoreCase));
            if (account is not null) account.IsEnabled = true;
            return Task.CompletedTask;
        });
    }

    public Task ResetStudentPasswordAsync(string id, string newPassword) => _authenticationService.SetStudentPasswordAsync(id, newPassword);
    public Task SetStudentAccountEnabledAsync(string id, bool enabled) => _authenticationService.SetStudentEnabledAsync(id, enabled);
    private static bool IsArchived(SchoolData data, string id) => data.StudentDetails.FirstOrDefault(d => d.StudentId == id)?.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase) == true;
    private static string GenerateNextId(IEnumerable<Student> students) { var next = students.Select(s => s.Id).Where(id => id.StartsWith("BHS", StringComparison.OrdinalIgnoreCase)).Select(id => id[3..]).Select(v => int.TryParse(v, out var n) ? n : 0).DefaultIfEmpty(0).Max() + 1; return $"BHS{next:000000}"; }
    private static string GenerateNextAdmissionNumber(IEnumerable<StudentDetails> details) { var next = details.Select(d => d.AdmissionNumber).Where(v => v.StartsWith("ADM", StringComparison.OrdinalIgnoreCase)).Select(v => v[3..]).Select(v => int.TryParse(v, out var n) ? n : 0).DefaultIfEmpty(0).Max() + 1; return $"ADM{next:000000}"; }
}
