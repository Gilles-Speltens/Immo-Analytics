using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class Session
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime Duration { get; set; }

        public User? User { get; set; }

        public ICollection<HitPage>? HitPages { get; set; }
    }
}
