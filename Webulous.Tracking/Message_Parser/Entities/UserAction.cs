using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace Message_Parser.Entities
{
    public class UserAction
    {
        public int? Id { get; set; }

        public required DateTime Time { get; set; }

        public required int PageId { get; set; }

        public required ActionsType ActionType { get; set; }

        public string? ActionParameter { get; set; }
    }
}
