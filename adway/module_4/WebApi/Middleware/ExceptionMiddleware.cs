using System.Text.Json;
using WebApi.Common;
using WebApi.Common.Exceptions;

namespace WebApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning("API error {ErrorCode}: {Message}", ex.ErrorCode, ex.Message);

            await WriteErrorResponse(
                context,
                ex.StatusCode,
                ex.ErrorCode,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception");

            await WriteErrorResponse(
                context,
                500,
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string errorCode,
        string errorMessage)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ReturnResult<object>
        {
            StatusCode = statusCode,
            IsSuccess = false,
            Result = null,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            TraceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}