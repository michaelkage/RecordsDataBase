namespace BombiHighSchool.App.Models;

public sealed class AcademicPeriod
{
    public string Session { get; set; } = "2026/2027";
    public string Term { get; set; } = "First Term";

    public override string ToString() => $"{Session} • {Term}";
}

public static class SchoolRules
{
    public static readonly string[] Arms =
    ["Gold", "Emerald", "Jade", "Silver", "Topaz", "Platinum", "White", "Crimson", "Diamond"];

    public static readonly string[] Terms = ["First Term", "Second Term", "Third Term"];
}
