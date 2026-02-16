using System.Net;

namespace Tracking_API.Model
{
    // Source - https://stackoverflow.com/a/10527155
    // Posted by Michael Liu, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-02-12, License - CC BY-SA 3.0

    public class IPSubnet
    {
        private readonly IPAddress _ip;
        private readonly byte[] _address;
        private readonly int _prefixLength;

        public IPSubnet(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            string[] parts = value.Split('/');

            if (parts.Length == 1)
            {
                // Pas de CIDR, traiter comme une IP unique
                _ip = IPAddress.Parse(parts[0]);
                _address = _ip.GetAddressBytes();
                _prefixLength = _address.Length == 4 ? 32 : 128; // IPv4 -> /32, IPv6 -> /128
            }
            else if (parts.Length == 2)
            {
                _ip = IPAddress.Parse(parts[0]);
                _address = _ip.GetAddressBytes();
                _prefixLength = Convert.ToInt32(parts[1], 10);
            }
            else
            {
                throw new ArgumentException("Invalid CIDR notation.", nameof(value));
            }
        }

        public bool Contains(string address)
        {
            return this.Contains(IPAddress.Parse(address).GetAddressBytes());
        }

        public bool Contains(byte[] address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            if (address.Length != _address.Length)
                return false; // IPv4/IPv6 mismatch

            int index = 0;
            int bits = _prefixLength;

            for (; bits >= 8; bits -= 8)
            {
                if (address[index] != _address[index])
                    return false;
                ++index;
            }

            if (bits > 0)
            {
                int mask = (byte)~(255 >> bits);
                if ((address[index] & mask) != (_address[index] & mask))
                    return false;
            }

            return true;
        }

        public string getIp()
        {
            return String.Concat(_ip.ToString(), "/", _prefixLength.ToString());
        }
    }

}
