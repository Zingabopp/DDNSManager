using DDNSManager.Lib.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace DDNSManager.Service
{
    [ProviderAlias("FileLogger")]
    internal class FileLoggerProvider : ILoggerProvider
    {
        private readonly string? FilePath;
        public FileLoggerProvider(CommandLineOptions args, IOptions<FileLoggerProviderOptions> options)
        {
            string? logPath = args.LoggingPath;
            if (string.IsNullOrWhiteSpace(logPath))
            {
                FilePath = options.Value.FilePath;
            }
            if (!string.IsNullOrWhiteSpace(logPath))
            {
                FilePath = Path.GetFullPath(logPath);
            }
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(FilePath);
        }

        public void Dispose()
        {

        }
    }

    public class FileLoggerProviderOptions
    {
        public virtual string? FilePath { get; set; }
    }

    public class FileLogger : ILogger
    {
        private static readonly object _locker = new object();
        private readonly string? FilePath;
        private readonly string? DirectoryPath;
        public FileLogger(string? filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
                FilePath = filePath;
            if (!string.IsNullOrWhiteSpace(FilePath))
                DirectoryPath = Path.GetDirectoryName(FilePath);
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        [MemberNotNullWhen(true, nameof(FilePath))]
        [MemberNotNullWhen(true, nameof(DirectoryPath))]
        public bool IsEnabled(LogLevel logLevel)
        {
            if (FilePath == null)
                return false;
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            try
            {
                lock (_locker)
                {
                    Directory.CreateDirectory(DirectoryPath);
                    string? logRecord = string.Format("{0} [{1}] {2} {3}", "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]", logLevel.ToString(), formatter(state, exception), exception != null ? exception.StackTrace : "");
                    using (StreamWriter? streamWriter = new StreamWriter(FilePath, true))
                    {
                        streamWriter.WriteLine(logRecord);
                    }
                }
            }
            catch { }
        }
    }

    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, Action<FileLoggerProviderOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
