using Common;
using System.Collections;
using System.Net;
using System.Text.Json;

namespace Interface_Gestion_API.Models
{
    public class WhiteListManager
    {
        private WhiteListViewModel _whiteList;

        public WhiteListManager(WhiteListViewModel whiteList)
        {
            _whiteList = whiteList;
        }

        public void RefreshWhiteListIfEmpty(List<string> ips, List<string> domains)
        {
            if (!_whiteList.IPv4.Any() && !_whiteList.IPv6.Any() && !_whiteList.Domains.Any())
            {
                RefreshWhiteList(ips, domains);
            }
        }

        public void RefreshWhiteList(List<string> ips, List<string> domains)
        {
            _whiteList = CreateWhiteList(domains, ips);
        }

        public void RefreshIpList(List<string> ips)
        {
            (_whiteList.IPv4, _whiteList.IPv6) = CreateIps(ips);
        }

        public void RefreshDomainList(List<string> domain)
        {
            _whiteList.Domains = domain;
        }

        public void ClearWhitelist()
        {
            _whiteList.IPv4.Clear();
            _whiteList.IPv6.Clear();
            _whiteList.Domains.Clear();
        }

        public bool IsEmpty()
        {
            return !_whiteList.IPv4.Any() && !_whiteList.IPv6.Any() && !_whiteList.Domains.Any();
        }

        public WhiteListViewModel GetWhiteList()
        {
            return _whiteList;
        }

        private WhiteListViewModel CreateWhiteList(List<string> domainsList, List<string> ipsList)
        {
            var (ipv4List, ipv6List) = CreateIps(ipsList);

            return new WhiteListViewModel
            {
                Domains = domainsList,
                IPv4 = ipv4List,
                IPv6 = ipv6List,
            };
        }

        private (List<IPSubnet> IPv4, List<IPSubnet> IPv6) CreateIps(List<string> list)
        {
            var ipv4List = new List<IPSubnet>();
            var ipv6List = new List<IPSubnet>();

            if (list is null)
                return (ipv4List, ipv6List);

            foreach (var i in list)
            {
                if (i.Contains(":"))
                {
                    ipv6List.Add(new IPSubnet(i));
                }
                else
                {
                    ipv4List.Add(new IPSubnet(i));
                }
            }

            return (ipv4List, ipv6List);
        }
    }
}
