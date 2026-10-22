using System.Text.Json;

namespace WebApi.Common;

public static class ErrorResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string errorMessage)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

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

        var json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }
}
