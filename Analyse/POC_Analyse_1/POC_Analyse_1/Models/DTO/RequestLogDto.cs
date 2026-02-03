namespace POC_Analyse_1.Models.DTO
{
    public record RequestLogDto(
        string UserAgent,
        DateTime Date,
        string CurrentPath,
        string? UrlReferrer,
        string SessionId,
        string? Action
    );
}
