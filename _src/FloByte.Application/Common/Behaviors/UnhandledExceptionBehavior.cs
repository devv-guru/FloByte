using Mediator;
using Microsoft.Extensions.Logging;

namespace FloByte.Application.Common.Behaviors;

public class UnhandledExceptionBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ILogger<UnhandledExceptionBehavior<TMessage, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TMessage, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            var requestName = typeof(TMessage).Name;
            _logger.LogError(
                ex,
                "Unhandled Exception for Request {RequestName} {@Request}",
                requestName,
                message);

            throw;
        }
    }
}
