using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace Message_Parser.Entities
{
    public class UserAction
    {
        public int Id { get; set; }

        public DateTime Time { get; set; }

        public string PageId { get; set; } = null!;

        public ActionsType ActionType { get; set; }

        public string? ActionParameter { get; set; }

        public HitPage? HitPage { get; set; }
    }
}
