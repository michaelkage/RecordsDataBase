namespace BombiHighSchool.App.Models;

public class SchoolData
{
    public List<Student> Students { get; set; } = [];
    public List<StudentDetails> StudentDetails { get; set; } = [];
    public List<Subject> Subjects { get; set; } = [];
    public List<SubjectEnrollment> Enrollments { get; set; } = [];
    public List<Score> Scores { get; set; } = [];
    public List<UserAccount> Users { get; set; } = [];
    public AcademicPeriod CurrentAcademicPeriod { get; set; } = new();

    // Monotonic counters prevent student/admission identifiers from being reused after deletion.
    // Zero means the value has not yet been initialized and will be migrated from existing records.
    public int NextStudentNumber { get; set; }
    public int NextAdmissionNumber { get; set; }
}
