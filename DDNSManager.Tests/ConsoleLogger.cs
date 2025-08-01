using Microsoft.Extensions.Logging;
using System;

namespace DDNSManager.Tests;
public class ConsoleLogger : ILogger
{
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= MinimumLogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            string message = formatter(state, exception);
            if (exception != null)
            {
                message += Environment.NewLine + exception.ToString();
            }
            Console.WriteLine($"[{logLevel}] {message}");
        }
    }
}

public class ConsoleLogger<T> : ConsoleLogger, ILogger<T>
{
    public ConsoleLogger() : base()
    {
    }
    public ConsoleLogger(LogLevel minimumLogLevel) : base()
    {
        MinimumLogLevel = minimumLogLevel;
    }
}
