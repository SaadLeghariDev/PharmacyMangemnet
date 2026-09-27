using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.Exceptions;

namespace PharmacyManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        // Let Swagger / OpenAPI schema generation surface raw errors instead of
        // ApiResponse JSON (Swagger UI rejects that as "no valid version field").
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (status, message, errors) = exception switch
        {
            ValidationAppException vae => (vae.StatusCode, vae.Message, vae.Errors),
            ValidationException fve => (400, "Validation failed", fve.Errors.Select(e => e.ErrorMessage).Distinct().ToList()),
            DbUpdateConcurrencyException => (409, "Concurrency conflict. Reload and retry.", (IReadOnlyList<string>?)null),
            AppException ae => (ae.StatusCode, ae.Message, (IReadOnlyList<string>?)null),
            UnauthorizedAccessException => (401, "Unauthorized", null),
            _ => (500, "An unexpected error occurred.", null)
        };

        if (status >= 500)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled exception: {Message}", message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        var payload = ApiResponse.Fail(message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
