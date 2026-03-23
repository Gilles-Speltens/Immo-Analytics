using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class Session
    {
        public required int Id { get; set; }
        public string? SessionId { get; set; }
        public required string Site { get; set; }
        public string? UserId { get; set; }
        public required string UserIp { get; set; }
        public required string LanguageBrowser { get; set; }
        public required string UserAgent { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }

    }
}
