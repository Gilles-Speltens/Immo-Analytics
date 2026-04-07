using Common;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using Tracking_API.Services;

namespace Tracking_API.Infrastructures
{
    public class IPManager : WhitelistManager<IPSubnet>
    {
        public IPManager(IFileManager fileManager) : base(fileManager) { }

        protected override IPSubnet Convert(string input)
            => new IPSubnet(input);

        protected override bool Matches(IPSubnet item, string input)
            => item.Matches(input);

        protected override bool Contains(IPSubnet item, string input)
            => item.Contains(input);

        protected override string Serialize(IPSubnet item)
            => item.GetIp();
    }
}
