using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FloByte.API.Services;

public static class UIHealthChecksResponseWriter
{
    public static Task WriteDetailedResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            timestamp = DateTime.UtcNow,
            entries = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                data = e.Value.Data,
                error = e.Value.Exception?.Message
            })
        };

        return context.Response.WriteAsJsonAsync(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
