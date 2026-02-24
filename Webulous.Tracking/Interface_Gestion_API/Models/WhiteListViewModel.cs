using Common;

namespace Interface_Gestion_API.Models
{
    public class WhiteListViewModel
    {
        public List<IPSubnet> IpV4 { get; set; }
        public List<IPSubnet> IpV6 { get; set; }
        public List<string> Domains { get; set; }
    }
}
