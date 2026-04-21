using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UserActions
{
    public class EstateSearchParameters : ActionParametersBase
    {
        public bool ToSell;
        public string EstateType;
        public string Locality;
        public int MinPrice;
        public int MaxPrice;
        public int BedroomNb;
    }
}
