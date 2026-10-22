using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WebClient.Models;

namespace WebClient.Services;

public interface IUniversityApiClient
{
    Task<ApiResult<List<CourseV1Model>>> GetCoursesV1Async(string? search, CancellationToken cancellationToken);

    Task<ApiResult<List<CourseV2Model>>> GetCoursesV2Async(string? search, CancellationToken cancellationToken);

    Task<ApiResult<CourseV1Model>> GetCourseV1Async(int id, CancellationToken cancellationToken);

    Task<ApiResult<CourseV2Model>> GetCourseV2Async(int id, CancellationToken cancellationToken);

    Task<ApiResult<LoginResponseModel>> LoginAsync(string userName, string password, CancellationToken cancellationToken);

    Task<ApiResult<UserProfileModel>> GetProfileAsync(string accessToken, CancellationToken cancellationToken);

    Task<ApiResult<List<StudentCourseModel>>> GetStudentCoursesAsync(int studentId, string accessToken, CancellationToken cancellationToken);
}

public class UniversityApiClient : IUniversityApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<UniversityApiClient> _logger;

    public UniversityApiClient(
        HttpClient httpClient,
        ILogger<UniversityApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<ApiResult<List<CourseV1Model>>> GetCoursesV1Async(string? search, CancellationToken cancellationToken)
    {
        return SendAsync<List<CourseV1Model>>(
            new HttpRequestMessage(HttpMethod.Get, WithSearch("api/v1/courses", search)),
            cancellationToken);
    }

    public Task<ApiResult<List<CourseV2Model>>> GetCoursesV2Async(string? search, CancellationToken cancellationToken)
    {
        return SendAsync<List<CourseV2Model>>(
            new HttpRequestMessage(HttpMethod.Get, WithSearch("api/v2/courses", search)),
            cancellationToken);
    }

    public Task<ApiResult<CourseV1Model>> GetCourseV1Async(int id, CancellationToken cancellationToken)
    {
        return SendAsync<CourseV1Model>(
            new HttpRequestMessage(HttpMethod.Get, $"api/v1/courses/{id}"),
            cancellationToken);
    }

    public Task<ApiResult<CourseV2Model>> GetCourseV2Async(int id, CancellationToken cancellationToken)
    {
        return SendAsync<CourseV2Model>(
            new HttpRequestMessage(HttpMethod.Get, $"api/v2/courses/{id}"),
            cancellationToken);
    }

    public Task<ApiResult<LoginResponseModel>> LoginAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        {
            Content = JsonContent.Create(new { userName, password })
        };

        return SendAsync<LoginResponseModel>(request, cancellationToken);
    }

    public Task<ApiResult<UserProfileModel>> GetProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        return SendAsync<UserProfileModel>(
            WithToken(new HttpRequestMessage(HttpMethod.Get, "api/auth/me"), accessToken),
            cancellationToken);
    }

    public Task<ApiResult<List<StudentCourseModel>>> GetStudentCoursesAsync(int studentId, string accessToken, CancellationToken cancellationToken)
    {
        return SendAsync<List<StudentCourseModel>>(
            WithToken(new HttpRequestMessage(HttpMethod.Get, $"api/v1/students/{studentId}/courses"), accessToken),
            cancellationToken);
    }

    private async Task<ApiResult<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using (request)
        {
            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                var envelope = await ReadEnvelopeAsync<T>(response, cancellationToken);

                if (response.IsSuccessStatusCode && envelope is { IsSuccess: true, Result: not null })
                {
                    return ApiResult<T>.Success(envelope.Result, (int)response.StatusCode);
                }

                _logger.LogWarning(
                    "University API returned {StatusCode} for {Method} {Path}",
                    (int)response.StatusCode,
                    request.Method,
                    request.RequestUri);

                return ApiResult<T>.Failure(
                    (int)response.StatusCode,
                    envelope?.ErrorMessage ?? DescribeStatus(response.StatusCode));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    "University API is unavailable: {Message}",
                    ex.Message);

                return ApiResult<T>.Unavailable(
                    "Сервис University Course Management API недоступен. Проверьте, что API запущен, и повторите попытку.");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError("University API request timed out");

                return ApiResult<T>.Unavailable(
                    "API не ответил за отведенное время. Повторите попытку позже.");
            }
        }
    }

    private static async Task<ApiEnvelope<T>?> ReadEnvelopeAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/json")
        {
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DescribeStatus(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => "Запрошенный ресурс не найден.",
            HttpStatusCode.Unauthorized => "Требуется вход в систему.",
            HttpStatusCode.Forbidden => "Недостаточно прав для выполнения операции.",
            HttpStatusCode.BadRequest => "Некорректный запрос.",
            _ when (int)statusCode >= 500 => "Ошибка на стороне API. Повторите попытку позже.",
            _ => $"API вернул код {(int)statusCode}."
        };
    }

    private static string WithSearch(string path, string? search)
    {
        return string.IsNullOrWhiteSpace(search)
            ? path
            : $"{path}?search={Uri.EscapeDataString(search)}";
    }

    private static HttpRequestMessage WithToken(HttpRequestMessage request, string accessToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
