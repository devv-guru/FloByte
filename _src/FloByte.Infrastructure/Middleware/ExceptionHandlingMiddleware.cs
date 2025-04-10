using FloByte.Application.Common.Exceptions;
using FloByte.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace FloByte.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggingService _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILoggingService logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = GetStatusCode(exception),
            Title = GetTitle(exception),
            Detail = GetDetail(exception),
            Instance = context.Request.Path
        };

        if (_environment.IsDevelopment())
        {
            problem.Extensions["exception"] = new
            {
                exception.Message,
                exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        response.StatusCode = problem.Status.Value;
        await response.WriteAsync(json);
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ForbiddenAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ValidationException => "Validation Error",
            NotFoundException => "Resource Not Found",
            UnauthorizedAccessException => "Unauthorized",
            ForbiddenAccessException => "Forbidden",
            _ => "Server Error"
        };

    private static string GetDetail(Exception exception) =>
        exception switch
        {
            ValidationException validationEx => 
                string.Join(" ", validationEx.Errors.Select(e => e.Value)),
            _ => exception.Message
        };
}
