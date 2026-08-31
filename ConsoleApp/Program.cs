using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleAppStudentdatabase
{
    public class Score
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int Grade { get; set; }

        public Score(int studentId, int subjectId, int grade)
        {
            StudentID = studentId;
            SubjectID = subjectId;
            Grade = grade;
        }
    }
    public class Student
    {
        public int StudentID { get; set; }
        public string StudentPassword { get; set; }
        public string Name { get; set; }
        public List<int> OfferedSubjectIDs { get; set; } = new List<int>();

        public Student(int id, string name, string Password)
        {
            StudentID = id;
            Name = name;
            StudentPassword = Password;
        }
    }
    public class Subject
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }

        public Subject(int id, string name)
        {
            SubjectID = id;
            SubjectName = name;
        }
    }
    class Program
    {
        private static Student[] students = new Student[0];
        private static Subject[] subjects = new Subject[0];
        private static Score[] scores = new Score[0];
        private const string StudentFile = "Student.txt";
        private const string SubjectFile = "Subject.txt";
        private const string ScoresFile = "Scores.txt";
        private const string PasswordFile = "AdminPassword.txt";
        // private const string StudentPasswordFile = "StudentPassword.txt";
        private static string AdminPassword = "Messi";
        //private static string StudentsPassword = "Yamal";
        static void Main(string[] args)
        {
            LoadMemory();
            bool finish = true;
            while (finish)
            {
                Console.WriteLine("\n--- Main Menu ---");
                Console.WriteLine("1. Admin ");
                Console.WriteLine("2. Student ");
                Console.WriteLine("3. Exit");
                Console.Write("Choose an option: ");
                int.TryParse(Console.ReadLine(), out int Choice);
                if (Choice == 1) Admin();
                else if (Choice == 2) StudentView();
                else if (Choice == 3) finish = false;
                else Console.WriteLine("Invalid option....");
            }
        }
        static string GetLetterGrade(int numericGrade)
        {
            if (numericGrade >= 80) return "A";
            if (numericGrade >= 70) return "B";
            if (numericGrade >= 60) return "C";
            if (numericGrade >= 50) return "P";
            return "F";
        }
        static void ViewStudentGrades(Student student)
        {
            Console.WriteLine($"\nName: {student.Name}");

            var offeredSubjects = subjects.Where(sub => student.OfferedSubjectIDs.Contains(sub.SubjectID)).ToArray();
            if (offeredSubjects.Length == 0)
            {
                Console.WriteLine("You are not offering any subjects currently.");
                return;
            }

            PrintTableHeader();
            var studentScores = scores.Where(s => s.StudentID == student.StudentID).ToArray();
            foreach (var subject in offeredSubjects)
            {
                var match = studentScores.FirstOrDefault(s => s.SubjectID == subject.SubjectID);
                int currentGrade = match != null ? match.Grade : 0;
                string letterGrade = GetLetterGrade(currentGrade);

                PrintTableRow(subject.SubjectName, currentGrade.ToString(), letterGrade);
            }
            PrintTableFooter();
        }
        static void Admin()
        {
            Console.Write("What's the Password: ");
            string password = Console.ReadLine();
            if (password == AdminPassword)
            {
                bool adminSession = true;
                while (adminSession)
                {
                    Console.WriteLine("\n--- Admin Menu ---");
                    Console.WriteLine("1. View All Results");
                    Console.WriteLine("2. Manage Student");
                    Console.WriteLine("3. Add New Subject");
                    Console.WriteLine("4. Change Admin Password");
                    Console.WriteLine("5. Logout");
                    int.TryParse(Console.ReadLine(), out int adminChoice);

                    if (adminChoice == 1) DisplayAllResults();
                    else if (adminChoice == 2) StudentManage();
                    else if (adminChoice == 3) AddNewSubject();
                    else if (adminChoice == 4) ChangePassword();
                    else if (adminChoice == 5) adminSession = false;
                    else Console.WriteLine("Invalid admin option.");
                }
            }
            else
            {
                Console.WriteLine("Wrong!!!!");
            }
        }
        static void AdminManageStudentSubjects()
        {
            Console.Write("\nEnter Student ID: ");
            if (!int.TryParse(Console.ReadLine(), out int sId) || !students.Any(s => s.StudentID == sId))
            {
                Console.WriteLine("Invalid or non-existent Student ID.");
                return;
            }

            Student student = students.First(s => s.StudentID == sId);
            ManageOfferedSubjects(student);
        }
        static void ManageOfferedSubjects(Student student)
        {
            Console.WriteLine($"\n--- Managing Subjects for {student.Name} ---");
            Console.WriteLine("1. Offer a new subject");
            Console.WriteLine("2. Drop a subject");
            Console.Write("Choose action: ");
            int.TryParse(Console.ReadLine(), out int action);

            if (action == 1)
            {
                var availableToOffer = subjects.Where(sub => !student.OfferedSubjectIDs.Contains(sub.SubjectID)).ToArray();
                if (availableToOffer.Length == 0)
                {
                    Console.WriteLine("No new subjects available to offer.");
                    return;
                }

                Console.WriteLine("\nAvailable subjects to offer:");
                foreach (var sub in availableToOffer)
                {
                    Console.WriteLine($"ID: {sub.SubjectID} | Name: {sub.SubjectName}");
                }

                Console.Write("Enter Subject ID to offer: ");
                if (int.TryParse(Console.ReadLine(), out int subId) && availableToOffer.Any(s => s.SubjectID == subId))
                {
                    student.OfferedSubjectIDs.Add(subId);
                    SaveMemory();
                    Console.WriteLine("Subject added to offerings successfully!");
                }
                else
                {
                    Console.WriteLine("Invalid Subject ID.");
                }
            }
            else if (action == 2)
            {
                if (student.OfferedSubjectIDs.Count == 0)
                {
                    Console.WriteLine("This student isn't offering any subjects to drop.");
                    return;
                }

                Console.WriteLine("\nCurrently offered subjects:");
                foreach (var id in student.OfferedSubjectIDs)
                {
                    var sub = subjects.FirstOrDefault(s => s.SubjectID == id);
                    if (sub != null) Console.WriteLine($"ID: {sub.SubjectID} | Name: {sub.SubjectName}");
                }

                Console.Write("Enter Subject ID to drop: ");
                if (int.TryParse(Console.ReadLine(), out int subId) && student.OfferedSubjectIDs.Contains(subId))
                {
                    student.OfferedSubjectIDs.Remove(subId);
                    List<Score> scoreList = scores.ToList();
                    scoreList.RemoveAll(s => s.StudentID == student.StudentID && s.SubjectID == subId);
                    scores = scoreList.ToArray();
                    SaveMemory();
                    Console.WriteLine("Subject dropped successfully!");
                }
                else { Console.WriteLine("Invalid Subject ID."); }
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }
        }
        static void DisplayAllResults()
        {
            Console.WriteLine("\n--- All Student Results ---");
            if (students.Length == 0)
            {
                Console.WriteLine("No students registered.");
                return;
            }
            foreach (var student in students)
            {
                Console.WriteLine($"\nStudent ID: {student.StudentID} | Name: {student.Name}");
                var offeredSubjects = subjects.Where(sub => student.OfferedSubjectIDs.Contains(sub.SubjectID)).ToArray();
                if (offeredSubjects.Length == 0)
                {
                    Console.WriteLine("  Not offering any subjects.");
                    continue;
                }
                PrintTableHeader();
                var studentScores = scores.Where(s => s.StudentID == student.StudentID).ToArray();
                foreach (var subject in offeredSubjects)
                {
                    var match = studentScores.FirstOrDefault(s => s.SubjectID == subject.SubjectID);
                    int grade = match != null ? match.Grade : 0;
                    PrintTableRow(subject.SubjectName, grade.ToString(), GetLetterGrade(grade));
                }
                PrintTableFooter();
            }
        }
        static void PrintTableHeader()
        {
            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"{"Subject",-20} | {"Score",-7} | {"Grade",-5}");
            Console.WriteLine(new string('-', 40));
        }
        static void PrintTableRow(string subject, string score, string letter)
        {
            Console.WriteLine($"{subject,-20} | {score,-7} | {letter,-5}");
        }
        static void PrintTableFooter()
        {
            Console.WriteLine(new string('-', 40));
        }
        static void ModifyStudentGrade()
        {
            Console.Write("\nEnter Student ID: ");
            if (!int.TryParse(Console.ReadLine(), out int sId) || !students.Any(s => s.StudentID == sId))
            {
                Console.WriteLine("Invalid or non-existent Student ID.");
                return;
            }
            Student student = students.First(s => s.StudentID == sId);

            Console.Write("Enter Subject ID: ");
            if (!int.TryParse(Console.ReadLine(), out int subId) || !subjects.Any(s => s.SubjectID == subId))
            {
                Console.WriteLine("Invalid or non-existent Subject ID.");
                return;
            }
            if (!student.OfferedSubjectIDs.Contains(subId))
            {
                Console.WriteLine("Warning: This student hasn't offered this subject yet.");
            }
            Console.Write("Enter New Grade (0-100): ");
            if (int.TryParse(Console.ReadLine(), out int grade) && grade >= 0 && grade <= 100)
            {
                var scoreList = scores.ToList();
                var existing = scoreList.FirstOrDefault(s => s.StudentID == sId && s.SubjectID == subId);
                if (existing != null)
                {
                    existing.Grade = grade;
                }
                else
                {
                    scoreList.Add(new Score(sId, subId, grade));
                }
                scores = scoreList.ToArray();
                SaveMemory();
                Console.WriteLine("Grade updated successfully!");
            }
            else
            {
                Console.WriteLine("Invalid grade scale.");
            }
        }
        static void AddNewStudent()
        {
            Console.Write("\nEnter New Student ID: ");
            if (int.TryParse(Console.ReadLine(), out int newId))
            {
                if (students.Any(s => s.StudentID == newId))
                {
                    Console.WriteLine("Error: Student ID already exists.");
                    return;
                }
                Console.Write("Enter Name: ");
                string name = Console.ReadLine();
                Console.Write("Enter Password: ");
                string Password = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    students = students.Append(new Student(newId, name, Password)).ToArray();
                    SaveMemory();
                    Console.WriteLine("Student added!");
                }
            }
        }
        static void AddNewSubject()
        {
            Console.Write("\nEnter New Subject ID: ");
            if (int.TryParse(Console.ReadLine(), out int newId))
            {
                if (subjects.Any(s => s.SubjectID == newId))
                {
                    Console.WriteLine("Error: Subject ID already exists.");
                    return;
                }
                Console.Write("Enter Subject Name: ");
                string name = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    subjects = subjects.Append(new Subject(newId, name)).ToArray();
                    SaveMemory();
                    Console.WriteLine("Subject added!");
                }
            }
        }
        static void ChangePassword()
        {
            Console.Write("\nEnter current password: ");
            if (Console.ReadLine() == AdminPassword)
            {
                Console.Write("Enter new password: ");
                string newPass = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newPass))
                {
                    AdminPassword = newPass;
                    SaveMemory();
                    Console.WriteLine("Password saved permanently!");
                }
            }
            else
            {
                Console.WriteLine("Access Denied.");
            }
        }
        static void StudentView()
        {
            Console.Write("\nEnter your Student ID: ");
            if (!int.TryParse(Console.ReadLine(), out int inputId))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }
            Student student = students.FirstOrDefault(s => s.StudentID == inputId);
            Console.Write("Enter your Student password: ");
            string password = Console.ReadLine();
            if (student != null && student.StudentPassword == password)
            {
                bool studentSession = true;
                while (studentSession)
                {
                    Console.WriteLine($"\n--- Student Menu ({student.Name}) ---");
                    Console.WriteLine("1. View My Grades");
                    Console.WriteLine("2. Manage Offered Subjects (Offer/Drop)");
                    Console.WriteLine("3. Change Password");
                    Console.WriteLine("4. Edit My Details");
                    Console.WriteLine("5. Back to Main Menu");
                    Console.Write("Choose an option: ");
                    int.TryParse(Console.ReadLine(), out int studentChoice);
                    if (studentChoice == 1) ViewStudentGrades(student);
                    else if (studentChoice == 2) ManageOfferedSubjects(student);
                    else if (studentChoice == 3) StudentPasswordChange(inputId); 
                    else if (studentChoice == 4) EditStudentDetails(inputId);  
                    else if (studentChoice == 5) studentSession = false;
                    else
                    {
                        Console.WriteLine("Invalid option.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid Student ID or password. Access denied.");
            }
        }
        static void LoadMemory()
        {
            if (File.Exists(PasswordFile))
                AdminPassword = File.ReadAllText(PasswordFile).Trim();
            if (File.Exists(StudentFile))
            {
                var loadedStudents = new List<Student>();
                foreach (var line in File.ReadAllLines(StudentFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3 && int.TryParse(parts[0], out int id))
                    {
                        string name = parts[1];
                        string password = parts[2];
                        var student = new Student(id, name, password);
                        if (parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]))
                        {
                            student.OfferedSubjectIDs = parts
                            .Skip(4)           // Skips index 0 and 1
                            .Select(int.Parse)
                            .ToList();
                        }
                        loadedStudents.Add(student);
                    }
                }
                students = loadedStudents.ToArray();
            }
            if (File.Exists(SubjectFile))
            {
                subjects = File.ReadAllLines(SubjectFile)
                    .Select(line => line.Split(','))
                    .Where(parts => parts.Length == 2 && int.TryParse(parts[0], out _))
                    .Select(parts => new Subject(int.Parse(parts[0]), parts[1])).ToArray();
            }
            if (File.Exists(ScoresFile))
            {
                var loadedScores = new List<Score>();
                foreach (var line in File.ReadAllLines(ScoresFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3 && int.TryParse(parts[0], out int sId) && int.TryParse(parts[1], out int subId) && int.TryParse(parts[2], out int grade))
                    {
                        loadedScores.Add(new Score(sId, subId, grade));
                    }
                }
                scores = loadedScores.ToArray();
            }
        }
        static void RemoveStudent()
        {
            Console.Write("\nEnter Student ID to remove: ");
            if (int.TryParse(Console.ReadLine(), out int sId))
            {
                var studentToRemove = students.FirstOrDefault(s => s.StudentID == sId);
                if (studentToRemove == null)
                {
                    Console.WriteLine("\nStudent doesnt exist");
                    return;
                }
                Console.Write($"Are you sure you want to delete {studentToRemove.Name}?");
                string confirmation = Console.ReadLine().Trim().ToLower();
                if (confirmation != "yes")
                {
                    Console.WriteLine("\ndeletion Cancelled");
                    return;
                }
                students = students.Where(s => s.StudentID != sId).ToArray();
                scores = scores.Where(s => s.StudentID != sId).ToArray();
                SaveMemory();
                Console.WriteLine("Student and all associated records deleted successfully!");
            }
        }
        static void SaveMemory()
        {
            var studentLines = students.Select(s =>
            {
                string subjectsString = s.OfferedSubjectIDs != null && s.OfferedSubjectIDs.Count > 0
                ? string.Join(",", s.OfferedSubjectIDs) : "";
                return $"{s.StudentID},{s.Name},{s.StudentPassword},{s.OfferedSubjectIDs.Count},{subjectsString}".TrimEnd(',');
            });
            File.WriteAllLines(StudentFile, studentLines);
            File.WriteAllLines(SubjectFile, subjects.Select(s => $"{s.SubjectID},{s.SubjectName}"));
            File.WriteAllLines(ScoresFile, scores.Select(s => $"{s.StudentID},{s.SubjectID},{s.Grade}"));
            File.WriteAllText(PasswordFile, AdminPassword);
        }
        static void StudentPasswordChange(int inputId)
        {
            var student = students.FirstOrDefault(s => s.StudentID == inputId);
            Console.Write("Enter current student password: ");
            string password = Console.ReadLine();
            if (password == student.StudentPassword)
            {
                Console.Write("Enter new Password: ");
                string newpass = Console.ReadLine();
                student.StudentPassword = newpass;
                SaveMemory();
                Console.WriteLine("Success!!!");
            }
            else
            {
                Console.WriteLine("Fail :(");
            }
        }
        static void EditStudentDetails(int inputId)
        {
            var student = students.FirstOrDefault(s => s.StudentID == inputId);
            Console.Write($"Enter new name (leave blank to keep '{student.Name}'): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                student.Name = newName;
            }
            SaveMemory();
            Console.WriteLine("Student details updated successfully!");
        }
        static void StudentModify()
        {
            bool adminSession = true;
            while (adminSession)
            {
                Console.WriteLine("----Student Modify----");
                Console.WriteLine("1. Change a Student's Score");
                Console.WriteLine("2. Manage a Student's Offered Subjects");
                Console.WriteLine("3. Remove a Student");
                Console.WriteLine("4. BACK");
                int.TryParse(Console.ReadLine(), out int adminChoice);
                if (adminChoice == 1) { ModifyStudentGrade(); }
                if (adminChoice == 2) { AdminManageStudentSubjects(); }
                if (adminChoice == 3) { RemoveStudent(); }
                if (adminChoice == 4) { adminSession = false; }
            }

        }
        static void StudentManage()
        {
            bool adminSession = true;
            while (adminSession)
            {
                Console.WriteLine("----Student Management----");
                Console.WriteLine("1. Modify Student");
                Console.WriteLine("2. Add a new Student");
                Console.WriteLine("3. BACK");
                int.TryParse(Console.ReadLine(), out int adminChoice);
                if (adminChoice == 1) {StudentModify();}
                if (adminChoice == 2) {AddNewStudent();}
                if (adminChoice == 3) {adminSession = false;}
            }

        }
    }
}
