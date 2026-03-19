using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class HitPage
    {
        public int? Id { get; set; }
        public required DateTime Time { get; set; }
        public string? SessionPk { get; set; }
        public required string Url { get; set; }
        public string? Referrer { get; set; }
    }
}
