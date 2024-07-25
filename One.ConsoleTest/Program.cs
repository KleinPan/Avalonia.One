// See https://aka.ms/new-console-template for more information


using One.SimpleLog.Loggers;
using One.SimpleLog;
using One.SimpleLog.Extensions;

static class MyClass
{
    public static LoggerWrapper logger = LogManager.GetLogger();

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!"); 
        logger.WithPatternProperty("ThreadID", Thread.CurrentThread.ManagedThreadId.ToString()).Debug("Hello, World!2");
    }
}



