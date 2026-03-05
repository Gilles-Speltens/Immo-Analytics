using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class HitPage
    {
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public string? Referrer { get; set; }

        public int? SessionId { get; set; }

        public string Url { get; set; } = null!;

        public string? LanguageBrowser { get; set; }

        public string Website { get; set; } = null!;

        public string UserAction { get; set; } = null!;

        public Session? Session { get; set; }

        public Website? WebSite { get; set; }

        public ICollection<UserAction>? UserActions { get; set; }
    }
}
