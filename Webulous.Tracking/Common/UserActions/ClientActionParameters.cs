using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UserActions
{
    public class ClientActionParameters : ActionParametersBase
    {
        public ClientActionType ActionType;
        public string? details;
    }
}
