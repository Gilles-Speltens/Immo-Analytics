using Common.UserActions;

namespace Common
{
    public class RequestLogDto
    {
        public DateTime Date {  get; set; }
        public string? UserId { get; set; }
        public string UserIp { get; set; }
        public string? SessionId { get; set; }
        public string Url { get; set; }
        public string? UrlReferrer { get; set; }
        public ActionsType Action {  get; set; }
        public string? ActionParameters { get; set; }
        public string LanguageBrowser { get; set; }
        public string UserAgent { get; set; }
    }
}
