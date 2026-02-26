using Common;

namespace Interface_Gestion_API.Models
{
    public class WhiteListViewModel
    {
        public List<IPSubnet> IPv4 { get; set; } = new List<IPSubnet>();
        public List<IPSubnet> IPv6 { get; set; } = new List<IPSubnet>();
        public List<string> Domains { get; set; } = new List<string>();
    }
}
