using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UserActions
{
    public class EstateSearchs : UserActionsBase
    {
        public bool ToSell { get; set; }
        public string EstateType { get; set; }
        public string Locality { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public int BedroomNb { get; set; }
    }
}
