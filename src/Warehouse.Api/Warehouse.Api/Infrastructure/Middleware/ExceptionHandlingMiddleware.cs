using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Warehouse.Api.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log کردن خطا
            _logger.LogError(ex, "Unhandled exception occurred while processing the request.");

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // به طور پیش‌فرض 500
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var errorMessage = "An unexpected error occurred.";

        // اگر خواستی انواع خاص را مپ کنی، اینجا انجام بده:
        // if (exception is ArgumentException) { statusCode = 400; errorMessage = exception.Message; }

        var problem = new
        {
            statusCode,
            error = errorMessage,
            // در محیط Development جزئیات خطا را هم برمی‌گردانیم
            detail = _env.IsDevelopment() ? exception.ToString() : null,
            traceId = context.TraceIdentifier
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}
