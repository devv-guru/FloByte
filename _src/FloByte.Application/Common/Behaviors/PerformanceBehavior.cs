using Mediator;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FloByte.Application.Common.Behaviors;

public class PerformanceBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ILogger<PerformanceBehavior<TMessage, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;
    private const int _warningThresholdMs = 500;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TMessage, TResponse>> logger,
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
        var timer = Stopwatch.StartNew();
        var response = await next(message, cancellationToken);
        timer.Stop();

        var elapsedMilliseconds = timer.ElapsedMilliseconds;
        if (elapsedMilliseconds > _warningThresholdMs)
        {
            var requestName = typeof(TMessage).Name;
            var userId = _currentUser.UserId ?? "anonymous";

            _logger.LogWarning(
                "Long running request: {RequestName} ({ElapsedMilliseconds}ms) by {UserId}",
                requestName,
                elapsedMilliseconds,
                userId);
        }

        return response;
    }
}
