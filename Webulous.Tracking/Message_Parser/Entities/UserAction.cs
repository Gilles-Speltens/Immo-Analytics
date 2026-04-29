using System;
using System.Collections.Generic;
using System.Text;
using Common.UserActions;

namespace Message_Parser.Entities
{
    public class UserAction
    {
        public long? Id { get; set; }

        public required DateTime Time { get; set; }

        public required long PageId { get; set; }

        public required ActionsType ActionType { get; set; }
        public required UserActionsBase ActionParameter { get; set; }
    }
}
