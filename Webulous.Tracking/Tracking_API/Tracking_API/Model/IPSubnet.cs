using System.Net;

namespace Tracking_API.Model
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
        /// <exception cref="ArgumentException">Si la notation CIDR est invalide</exception>
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
            return String.Concat(_ip.ToString(), "/", _prefixLength.ToString());
        }
    }

}
