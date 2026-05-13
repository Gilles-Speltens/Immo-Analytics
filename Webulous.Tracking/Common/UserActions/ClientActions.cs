using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UserActions
{
    public class ClientActions : UserActionsBase
    {
        public ClientActionType ClientActionType { get; set; }
        public string? Details { get; set; }
    }
}
