using System.Collections;
using System.Collections.Generic;

namespace Tracking_API.Model
{
    public class IPManager
    {
        private readonly List<IPSubnet> _whiteList = new List<IPSubnet>();
        private readonly string _whiteListPath;

        public IPManager(string whiteListPath)
        {
            _whiteListPath = whiteListPath;
            AddIpsToSafeList(File.ReadAllLines(_whiteListPath));
        }

        public void AddIpToSafeList(string ip)
        {
            if (!_whiteList.Any(x => x.getIp().Equals(ip)))
            {
                _whiteList.Add(new IPSubnet(ip));
                SaveToFile();
            }
        }

        public void AddIpsToSafeList(string[] ips)
        {
            foreach (var ip in ips)
            {
                AddIpToSafeList(ip);
            }
        }

        public void RemoveIpToSafeList(string ip)
        {
            var parts = ip.Split('/');
            if (parts.Length == 1)
            {
                if(ip.Contains(":"))
                {
                    _whiteList.RemoveAll(subnet => subnet.getIp() == String.Concat(ip, "/128"));
                } else
                {
                    _whiteList.RemoveAll(subnet => subnet.getIp() == String.Concat(ip, "/32"));
                }
                SaveToFile();
            } else
            {
                _whiteList.RemoveAll(subnet => subnet.getIp() == ip);
                SaveToFile();
            }  
        }

        public bool IsInSafeList(string ip)
        {
            var isInList = false;
            foreach(var i in _whiteList)
            {
                if(i.Contains(ip))
                {
                    isInList = true; 
                    break; 
                }
            }
            return isInList;
        }

        public string[] getSafeList()
        {
            return _whiteList.Select(ip => ip.getIp()).ToArray();
        }

        private void SaveToFile()
        {
            using (StreamWriter writer = new StreamWriter(_whiteListPath, false)) // false = overwrite
            {
                foreach (var item in _whiteList)
                {
                    writer.WriteLine(item.getIp());
                }
            }
        }
    }
}
