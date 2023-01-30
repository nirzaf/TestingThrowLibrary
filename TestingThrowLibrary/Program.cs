// See https://aka.ms/new-console-template for more information

using Throw;

using static System.Console;

WriteLine("Testing Throw Validation Library.........");

try
{

    Student student = new FullTimeStudent();
    //student.Throw().IfNotType<Student>();
    student.Throw().IfType<Student>();

    string? data = "Hello, World!";
    data.ThrowIfNull("data is null");
    // var input = ReadLine();
    // input!.Trim().Throw().IfNullOrEmpty(p=>p, "Input is null or empty");
    WriteLine($"You entered: {data}");
    ReadLine();
}
catch (Exception exception)
{
    WriteLine(exception);
    ReadLine();
}


public class Student
{
    public string? Name { get; set; }
    public int Age { get; set; }
}

public class FullTimeStudent : Student
{
    public string? Course { get; set; }
}

