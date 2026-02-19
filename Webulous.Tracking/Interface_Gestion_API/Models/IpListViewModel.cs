using Common;

namespace Interface_Gestion_API.Models
{
    public class IpListViewModel
    {
        public List<IPSubnet> IpV4 { get; set; } = new List<IPSubnet>();
        public List<IPSubnet> IpV6 { get; set; } = new List<IPSubnet>();

    }
}
