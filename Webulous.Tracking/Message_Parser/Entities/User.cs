using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Message_Parser.Entities
{
    public class User
    {
        public int Id { get; set; }

        public ICollection<Session>? Sessions { get; set; }
    }
}
