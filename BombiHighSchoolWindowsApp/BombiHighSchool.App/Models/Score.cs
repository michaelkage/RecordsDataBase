namespace BombiHighSchool.App.Models;

public class Score
{
    public string Id { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string SubjectId { get; set; } = "";
    public double ScoreValue { get; set; }
    public double ExamScore { get; set; }
    public double Total => Math.Clamp(ScoreValue + ExamScore, 0, 100);
    public string Grade => Total switch
    {
        >= 75 => "A",
        >= 70 => "B",
        >= 60 => "C",
        >= 50 => "D",
        >= 45 => "E",
        _ => "F"
    };
}
