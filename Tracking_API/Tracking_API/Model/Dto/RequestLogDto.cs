namespace Tracking_API.Model.Dto
{
    public class RequestLogDto
    {
        public string UserId { get; set; }
        public string Url { get; set; }
        public string UrlReferrer { get; set; }
        public string Action { get; set; }
        public string LanguageBrowser { get; set; }
        public string SessionId { get; set; }
        public string UserAgent { get; set; }
    }
}
