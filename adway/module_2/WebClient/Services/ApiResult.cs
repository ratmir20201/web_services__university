namespace WebClient.Services;

public class ApiResult<T>
{
    public bool IsSuccess { get; private init; }

    public bool IsApiUnavailable { get; private init; }

    public int? StatusCode { get; private init; }

    public T? Data { get; private init; }

    public string? ErrorMessage { get; private init; }

    public static ApiResult<T> Success(T data, int statusCode)
    {
        return new ApiResult<T> { IsSuccess = true, Data = data, StatusCode = statusCode };
    }

    public static ApiResult<T> Failure(int statusCode, string message)
    {
        return new ApiResult<T> { StatusCode = statusCode, ErrorMessage = message };
    }

    public static ApiResult<T> Unavailable(string message)
    {
        return new ApiResult<T> { IsApiUnavailable = true, ErrorMessage = message };
    }
}
