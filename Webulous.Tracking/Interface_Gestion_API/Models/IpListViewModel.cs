namespace Interface_Gestion_API.Models
{
    public class IpListViewModel
    {
        public List<string> IpV4 { get; set; } = new List<string>();
        public List<string> IpV6 { get; set; } = new List<string>();

    }
}
