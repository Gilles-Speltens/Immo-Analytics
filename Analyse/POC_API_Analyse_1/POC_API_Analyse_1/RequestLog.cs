namespace POC_API_Analyse_1
{
    public record RequestLog(
        DateTimeOffset Date,
        string Url,
        string? UrlReferrer,
        string? Action,
        string? SessionId,
        string? UserAgent
    );
}
