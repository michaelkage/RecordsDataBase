namespace BombiHighSchool.App.Models;

public sealed class TranscriptRow
{
    public string Subject { get; init; } = "";
    public string Code { get; init; } = "";
    public double CA { get; init; }
    public double Exam { get; init; }
    public double Total { get; init; }
    public string Grade { get; init; } = "";
}
