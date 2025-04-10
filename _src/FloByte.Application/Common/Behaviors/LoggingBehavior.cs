using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FloByte.Application.Common.Behaviors;

public class LoggingBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ILogger<LoggingBehavior<TMessage, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TMessage, TResponse>> logger,
        ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var requestName = typeof(TMessage).Name;
        var userId = _currentUser.UserId ?? "anonymous";
        
        _logger.LogInformation(
            "Handling {RequestName} for user {UserId}",
            requestName,
            userId);

        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next(message, cancellationToken);
            
            timer.Stop();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                timer.ElapsedMilliseconds);
            throw;
        }
    }
}
