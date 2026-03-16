using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace Message_Parser.Entities
{
    public class UserAction
    {
        public int? Id { get; set; }

        public DateTime Time { get; set; }

        public int PageId { get; set; }

        public ActionsType ActionType { get; set; }

        public string? ActionParameter { get; set; }
    }
}
