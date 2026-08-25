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
}
