namespace WebClient.Services;

public class UniversityApiOptions
{
    public const string SectionName = "UniversityApi";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 10;
}
