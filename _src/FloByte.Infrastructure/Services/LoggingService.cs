using FloByte.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FloByte.Infrastructure.Services;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingService(
        ILogger<LoggingService> logger,
        ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public void LogTrace(string message, params object[] args) =>
        Log(LogLevel.Trace, null, message, args);

    public void LogDebug(string message, params object[] args) =>
        Log(LogLevel.Debug, null, message, args);

    public void LogInformation(string message, params object[] args) =>
        Log(LogLevel.Information, null, message, args);

    public void LogWarning(string message, params object[] args) =>
        Log(LogLevel.Warning, null, message, args);

    public void LogError(Exception? exception, string message, params object[] args) =>
        Log(LogLevel.Error, exception, message, args);

    public void LogCritical(Exception? exception, string message, params object[] args) =>
        Log(LogLevel.Critical, exception, message, args);

    private void Log(
        LogLevel logLevel,
        Exception? exception,
        string message,
        object[] args,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var userId = _currentUser.UserId;
        var enrichedMessage = $"[User: {userId ?? "Anonymous"}] {message}";

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = userId ?? "Anonymous",
            ["MemberName"] = memberName,
            ["FilePath"] = sourceFilePath,
            ["LineNumber"] = sourceLineNumber
        }))
        {
            if (exception != null)
            {
                _logger.Log(logLevel, exception, enrichedMessage, args);
            }
            else
            {
                _logger.Log(logLevel, enrichedMessage, args);
            }
        }
    }
}
