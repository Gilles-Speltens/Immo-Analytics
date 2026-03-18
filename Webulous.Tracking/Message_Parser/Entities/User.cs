using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Message_Parser.Entities
{
    public class User
    {
        public required string Ip { get; set; }
        public string? Id { get; set; }
    }
}
