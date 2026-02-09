namespace Mini_Site_Web.Models.Dto
{
    public class RequestLogDto
    {
        public DateTimeOffset Date { get; set; }
        public string Url { get; set; }
        public string UrlReferrer { get; set; }
        public string Action { get; set; }
        public string SessionId { get; set; }
        public string UserAgent { get; set; }
    }
}
