namespace BombiHighSchool.App.Models;

public class SchoolData
{
    public List<Student> Students { get; set; } = [];
    public List<StudentDetails> StudentDetails { get; set; } = [];
    public List<Subject> Subjects { get; set; } = [];
    public List<Score> Scores { get; set; } = [];
}
