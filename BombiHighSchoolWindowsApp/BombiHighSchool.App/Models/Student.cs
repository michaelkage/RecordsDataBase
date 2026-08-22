namespace BombiHighSchool.App.Models;

public class Student
{
    public string Id { get; set; } = "";
    public string Surname { get; set; } = "";
    public string OtherNames { get; set; } = "";
    public string Department { get; set; } = "";
    public string ClassLevel { get; set; } = "";
    public string ClassArm { get; set; } = "";
    public string Password { get; set; } = "";

    public List<StudentSubject> Subjects { get; set; } = [];
}

public class StudentSubject
{
    public string Name { get; set; } = "";
    public double? Score { get; set; }
}