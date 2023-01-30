// See https://aka.ms/new-console-template for more information

using Throw;

using static System.Console;

WriteLine("Hello, World!");

try
{
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

