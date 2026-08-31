namespace BombiHighSchool.App.Models;

public sealed class StudentPortalSnapshot
{
    public Student Student { get; init; } = new();
    public StudentDetails Details { get; init; } = new();
    public List<StudentScoreRow> Scores { get; init; } = [];
    public double Average => Scores.Count == 0 ? 0 : Scores.Average(x => x.Total);
    public int Position { get; init; }
    public int ClassSize { get; init; }
    public int DepartmentPosition { get; init; }
    public int DepartmentSize { get; init; }
}

public sealed class StudentScoreRow
{
    public string SubjectName { get; init; } = "";
    public string SubjectCode { get; init; } = "";
    public double ContinuousAssessment { get; init; }
    public double Exam { get; init; }
    public double Total { get; init; }
    public string Grade { get; init; } = "";
}
