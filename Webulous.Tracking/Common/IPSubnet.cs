using Common.Exceptions;
using System.Net;
using System.Net.Sockets;

namespace Common
{
    // Source - https://stackoverflow.com/a/10527155
    // Posted by Michael Liu, modified by community. See post 'Timeline' for change history
    // Retrieved 2026-02-12, License - CC BY-SA 3.0

    /// <summary>
    /// Représente un IP en IPv4 ou IPv6, avec sa range (notation CIDR).
    /// Permet de vérifier si une IP donnée appartient à sa range.
    /// </summary>
    public class IPSubnet
    {
        /// <summary>
        /// Adresse IP de base du subnet.
        /// </summary>
        private readonly IPAddress _ip;

        /// <summary>
        /// Représentation binaire de l'adresse IP.
        /// </summary>
        private readonly byte[] _address;

        /// <summary>
        /// Longueur du préfixe CIDR (ex: 32 pour IPv4 unique, 128 pour IPv6 unique, ou autre si spécifié).
        /// </summary>
        private readonly int _prefixLength;

        /// <summary>
        /// Crée un IPSubnet à partir d'une chaîne de caractères.
        /// La chaîne peut être une IP seule ("192.168.0.1") ou en CIDR ("192.168.0.0/24").
        /// </summary>
        /// <param name="value">IP ou subnet en notation standard ou CIDR</param>
        /// <exception cref="ArgumentNullException">Si la valeur est nulle</exception>
        /// <exception cref="InvalidIpException">Si la notation CIDR est invalide</exception>
        public IPSubnet(string value)
        {
            if (value == null)
                throw new ArgumentNullException("Ip can't be null");

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
                var prefix = Convert.ToInt32(parts[1], 10);
                int maxPrefix = parts[0].Contains(":") ? 128 : 32;

                if (prefix < 0 || prefix > maxPrefix)
                    throw new InvalidIpException($"Prefix must be between 0 and {maxPrefix} for {(maxPrefix == 128 ? "IPv6" : "IPv4")}");

                _ip = IPAddress.Parse(parts[0]);
                _address = _ip.GetAddressBytes();
                _prefixLength = prefix;

            }
            else
            {
                throw new InvalidIpException("Invalid CIDR notation.");
            }
        }

        /// <summary>
        /// Vérifie si une IP sous forme de chaîne appartient à ce subnet.
        /// </summary>
        /// <param name="address">IP à tester (ex: "192.168.0.5")</param>
        /// <returns>true si l'IP est contenue dans le subnet, false sinon</returns>
        /// <exception cref="ArgumentNullException">Si l'adresse est nulle</exception>
        public bool Contains(string address)
        {
            return this.Contains(IPAddress.Parse(address).GetAddressBytes());
        }

        /// <summary>
        /// Vérifie si une IP sous forme de tableau de bytes appartient à ce subnet.
        /// </summary>
        /// <param name="address">IP à tester sous forme de bytes</param>
        /// <returns>true si l'adresse est contenue dans le subnet, false sinon</returns>
        /// <exception cref="ArgumentNullException">Si le tableau d'adresse est nul</exception>
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

        /// <summary>
        /// Retourne la représentation du subnet sous forme de chaîne (ex: "192.168.0.1/32").
        /// </summary>
        /// <returns>IP + préfixe CIDR sous forme de string</returns>
        public string GetIp()
        {
            int defaultPrefix = _ip.AddressFamily == AddressFamily.InterNetwork
                    ? 32
                    : 128;

            if(_prefixLength == defaultPrefix)
            {
                return _ip.ToString();
            } 
            else
            {
                return String.Concat(_ip.ToString(), "/", _prefixLength.ToString());
            }
        }

        /// <summary>
        /// Vérifie si une IP est égale à _ip.
        /// </summary>
        /// <param name="ip">L'IP à comparer.</param>
        /// <returns>true si elle sont identique (avec le même préfixe) et false si non</returns>
        public bool Matches(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            var parts = ip.Split('/');

            if (!IPAddress.TryParse(parts[0], out var parsedIp))
                return false;

            if (parts.Length == 2)
            {
                if (!int.TryParse(parts[1], out int prefix))
                    return false;

                return _ip.Equals(parsedIp) && _prefixLength == prefix;
            }
            else if (parts.Length == 1)
            {
                int defaultPrefix = parsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    ? 32
                    : 128;

                return _ip.Equals(parsedIp) && _prefixLength == defaultPrefix;
            }

            return false;
        }
    }

}
