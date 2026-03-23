using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class Site
    {
        public required string Domain { get; set; }

        public required DateTime DateWhenAdded { get; set; }

        public bool Certify { get; set; }
    }
}
