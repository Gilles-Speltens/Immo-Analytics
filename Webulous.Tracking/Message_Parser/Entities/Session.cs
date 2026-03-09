using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class Session
    {
        public string Id { get; set; }

        public string? UserId { get; set; }

        public DateTime? Duration { get; set; }

    }
}
