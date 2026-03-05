using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class Website
    {
        public string Domain { get; set; } = null!;

        public DateTime DateWhenAdded { get; set; }

        public bool Certify { get; set; }

        public ICollection<HitPage>? HitPages { get; set; }
    }
}
