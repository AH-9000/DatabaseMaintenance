using DatabaseMaintenance.Logging;

public class ConsoleLogger : IAppLogger
{
    public void Information(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO ] {message}");
        Console.ResetColor();
    }

    public void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN ] {message}");
        Console.ResetColor();
    }

    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
        Console.WriteLine("Press any key to continue...");
        Console.ReadLine();
    }
}