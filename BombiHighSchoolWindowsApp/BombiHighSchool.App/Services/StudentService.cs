using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public sealed class StudentService
{
    private readonly LocalDataService _dataService;
    private readonly AuthenticationService _authenticationService;

    public StudentService(LocalDataService? dataService = null, AuthenticationService? authenticationService = null)
    {
        _dataService = dataService ?? new LocalDataService();
        _authenticationService = authenticationService ?? new AuthenticationService(_dataService);
    }

    public async Task<List<Student>> GetAllAsync(bool includeArchived = false)
    {
        var data = await _dataService.LoadAsync();
        // Student.IsArchived is the single authoritative lifecycle flag.
        return data.Students.Where(s => includeArchived || !s.IsArchived).ToList();
    }

    public async Task<Student> AddAsync(string name, string gender, string classLevel, string arm, string department)
    {
        Student student = new();
        await _dataService.UpdateAsync(data =>
        {
            var studentNumber = AllocateNextStudentNumber(data);
            var admissionNumber = AllocateNextAdmissionNumber(data);
            student = new Student
            {
                Id = $"BHS{studentNumber:000000}",
                Name = name.Trim(),
                Age = 0,
                Gender = gender.Trim(),
                ClassLevel = classLevel.Trim(),
                IsArchived = false
            };
            data.Students.Add(student);
            data.StudentDetails.Add(new StudentDetails
            {
                StudentId = student.Id,
                Arm = arm.Trim(),
                Department = department.Trim(),
                AdmissionNumber = $"ADM{admissionNumber:000000}",
                Status = "Active"
            });
            return Task.CompletedTask;
        });

        try
        {
            await _authenticationService.EnsureStudentAccountAsync(student.Id, "Welcome123!");
        }
        catch
        {
            // Roll back the student if its required account could not be created.
            await _dataService.UpdateAsync(data =>
            {
                data.Students.RemoveAll(s => s.Id == student.Id);
                data.StudentDetails.RemoveAll(d => d.StudentId == student.Id);
                return Task.CompletedTask;
            });
            throw;
        }

        return student;
    }

    public async Task UpdateProfileAsync(Student student, StudentDetails details)
    {
        await _dataService.UpdateAsync(data =>
        {
            var existing = data.Students.FirstOrDefault(s => s.Id == student.Id)
                ?? throw new InvalidOperationException("The student could not be found.");

            existing.Name = student.Name.Trim();
            existing.Age = student.Age;
            existing.Gender = student.Gender.Trim();
            existing.ClassLevel = student.ClassLevel.Trim();
            // Editing profile data MUST NOT change lifecycle state.

            var existingDetails = data.StudentDetails.FirstOrDefault(d => d.StudentId == student.Id);
            if (existingDetails is null)
            {
                details.StudentId = student.Id;
                details.Status = existing.IsArchived ? "Archived" : "Active";
                data.StudentDetails.Add(details);
            }
            else
            {
                existingDetails.Arm = details.Arm.Trim();
                existingDetails.Department = details.Department.Trim();
                existingDetails.DateOfBirth = details.DateOfBirth.Trim();
                // Admission numbers are generated identifiers and may only be retained, never changed by editing.
                existingDetails.ParentName = details.ParentName.Trim();
                existingDetails.ParentPhone = details.ParentPhone.Trim();
                existingDetails.Address = details.Address.Trim();
                existingDetails.Email = details.Email.Trim();
                existingDetails.Status = existing.IsArchived ? "Archived" : (string.IsNullOrWhiteSpace(details.Status) ? "Active" : details.Status.Trim());
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

    private static int AllocateNextStudentNumber(SchoolData data)
    {
        if (data.NextStudentNumber <= 0)
            data.NextStudentNumber = MaxNumber(data.Students.Select(s => s.Id), "BHS");
        return ++data.NextStudentNumber;
    }

    private static int AllocateNextAdmissionNumber(SchoolData data)
    {
        if (data.NextAdmissionNumber <= 0)
            data.NextAdmissionNumber = MaxNumber(data.StudentDetails.Select(d => d.AdmissionNumber), "ADM");
        return ++data.NextAdmissionNumber;
    }

    private static int MaxNumber(IEnumerable<string> values, string prefix) => values
        .Where(v => v.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        .Select(v => v[prefix.Length..])
        .Select(v => int.TryParse(v, out var n) ? n : 0)
        .DefaultIfEmpty(0)
        .Max();
}
