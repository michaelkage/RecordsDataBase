namespace BombiHighSchool.App.Models;

public class SchoolData
{
    public AdminAccount Admin { get; set; } = new();

    public List<Student> Students { get; set; } = [];

    public List<string> Subjects { get; set; } = [];
}

public class AdminAccount
{
    public string Password { get; set; } = "admin";
}