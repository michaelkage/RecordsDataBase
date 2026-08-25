namespace BombiHighSchool.App.Models;

public class Student
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
    public string ClassLevel { get; set; } = "";
    public bool IsArchived { get; set; }
}
