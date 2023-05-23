
public static class Log
{
  public static void WriteLine(string? message = null, ConsoleColor? foregroundColor = null)
  {
    var defaultColor = Console.ForegroundColor;
    if (foregroundColor != null)
    {
      Console.ForegroundColor = (ConsoleColor)foregroundColor;
    }
    Console.WriteLine(message);
    if (foregroundColor != null)
    {
      Console.ForegroundColor = defaultColor;
    }
  }
}