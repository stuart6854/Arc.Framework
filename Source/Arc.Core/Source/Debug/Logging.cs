using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Arc.Core;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal,
    None,
}

public readonly struct LogEvent
{
    public DateTime Timestamp { get; }
    public LogLevel Level { get; }
    public string Category { get; }
    public string Message { get; }
    public Exception? Exception { get; }

    public string? FilePath { get; }
    public string? MemberName { get; }
    public int LineNumber { get; }

    public LogEvent(
        DateTime timestamp,
        LogLevel level,
        string category,
        string message,
        Exception? exception = null,
        string? filePath = null,
        string? memberName = null,
        int lineNumber = 0
    )
    {
        Timestamp = timestamp;
        Level = level;
        Category = category;
        Message = message;
        Exception = exception;
        FilePath = filePath;
        MemberName = memberName;
        LineNumber = lineNumber;
    }
}

public interface ILogSink
{
    void Write(in LogEvent @event);
}

public interface ILogger
{
    string Name { get; internal set; }
    LogLevel Level { get; set; }
    List<ILogSink> Sinks { get; }

    void Trace(
        string msg,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0
    );
    void Debug(
        string msg,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0
    );
    void Info(
        string msg,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0
    );
    void Warn(
        string msg,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0
    );
    void Error(
        string msg,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0
    );
    void Fatal(
        string msg,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0
    );
}

public static class LoggerFactory
{
    private static readonly ConcurrentDictionary<string, ILogger> Loggers = [];
    private static readonly List<ILogSink> Sinks = [];

    public static void AddGlobalSink(ILogSink sink) => Sinks.Add(sink);

    public static IReadOnlyList<ILogSink> GetGlobalSinks() => Sinks;

    public static ILogger GetLogger(string name) => Loggers.GetOrAdd(name, n => new DefaultLogger(n));

    public static T GetLogger<T>(string name) where T : ILogger, new() => (T)Loggers.GetOrAdd(
        name, n =>
        {
            var l = new T { Name = n };
            return l;
        }
    );
}

#region Sinks

public sealed class ConsoleSink : ILogSink
{
    private readonly Lock _lock = new();
    private readonly Func<LogEvent, string> _formatter;

    public ConsoleSink(Func<LogEvent, string>? formatter = null) { _formatter = formatter ?? DefaultFormatter; }

    public void Write(in LogEvent evt)
    {
        var line = _formatter(evt);

        lock (_lock)
        {
            var oldFg = Console.ForegroundColor;
            var oldBg = Console.BackgroundColor;
            Console.ForegroundColor = LevelToFgColor(evt.Level);
            Console.BackgroundColor = LevelToBgColor(evt.Level);
            Console.WriteLine(line);
            Console.ForegroundColor = oldFg;
            Console.BackgroundColor = oldBg;
        }
    }

    public void Dispose() { }

    private static ConsoleColor LevelToFgColor(LogLevel level) => level switch {
        LogLevel.Trace => ConsoleColor.DarkGray,
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Info => ConsoleColor.White,
        LogLevel.Warn => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Fatal => ConsoleColor.White,
        _ => ConsoleColor.Gray // Default
    };

    private static ConsoleColor LevelToBgColor(LogLevel level) => level switch {
        LogLevel.Fatal => ConsoleColor.DarkRed,
        _ => ConsoleColor.Black // Default
    };

    private static string DefaultFormatter(LogEvent e)
    {
        var time = e.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff");
        var file = e.FilePath != null ? Path.GetFileName(e.FilePath) : "?";
        var member = e.MemberName ?? "?";
        var line = e.LineNumber;

        var header =
            $"[{time}] [{e.Level,5}] [{e.Category}] [{file}:{line} {member}]";

        var msg = header + " " + e.Message;

        if (e.Exception != null)
            msg += Environment.NewLine + e.Exception;

        return msg;
    }
}

#endregion

#region DefaultLogger

public sealed class DefaultLogger : ILogger
{
    public string Name { get; set; }
    public LogLevel Level { get; set; }
    public List<ILogSink> Sinks { get; } = [];

    public DefaultLogger(string name) { Name = name; }

    private void LogCore(
        LogLevel level,
        string msg,
        Exception? ex,
        string? file,
        string? member,
        int line
    )
    {
        var evt = new LogEvent(
            DateTime.UtcNow,
            level,
            Name,
            msg,
            ex,
            file,
            member,
            line
        );
        Sinks.ForEach(sink => sink.Write(evt));
        foreach (var sink in LoggerFactory.GetGlobalSinks())
            sink.Write(evt);
    }

    public void Trace(string msg, Exception? ex = null, string? file = null, string? member = null, int line = 0) =>
        LogCore(LogLevel.Trace, msg, ex, file, member, line);

    public void Debug(string msg, Exception? ex = null, string? file = null, string? member = null, int line = 0) =>
        LogCore(LogLevel.Debug, msg, ex, file, member, line);

    public void Info(string msg, Exception? ex = null, string? file = null, string? member = null, int line = 0) =>
        LogCore(LogLevel.Info, msg, ex, file, member, line);

    public void Warn(string msg, Exception? ex = null, string? file = null, string? member = null, int line = 0) =>
        LogCore(LogLevel.Warn, msg, ex, file, member, line);

    public void Error(string msg, Exception? ex = null, string? file = null, string? member = null, int line = 0) =>
        LogCore(LogLevel.Error, msg, ex, file, member, line);

    public void Fatal(string msg, Exception? ex = null, string? file = null, string? member = null, int line = 0) =>
        LogCore(LogLevel.Fatal, msg, ex, file, member, line);
}

#endregion