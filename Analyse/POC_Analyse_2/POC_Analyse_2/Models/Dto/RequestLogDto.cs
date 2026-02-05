namespace POC_Analyse_2.Models.Dto
{
    public record RequestLogDto(
        DateTimeOffset Date,
        string Url,
        string? UrlReferrer,
        string? Action,
        string? SessionId,
        string? UserAgent
    );
}
