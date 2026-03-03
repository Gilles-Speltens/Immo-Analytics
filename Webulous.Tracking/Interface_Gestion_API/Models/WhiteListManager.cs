using Common;
using System.Collections;
using System.Net;
using System.Text.Json;

namespace Interface_Gestion_API.Models
{
    /// <summary>
    /// Gère la liste blanche (whitelist) contenant des adresses IP (IPv4, IPv6)
    /// ainsi que des noms de domaines autorisés.
    /// 
    /// Cette classe permet :
    /// - D'initialiser la whitelist
    /// - De la rafraîchir totalement ou partiellement
    /// - De séparer automatiquement les IPv4 et IPv6
    /// - De vérifier si elle est vide
    /// - De la réinitialiser
    /// </summary>
    public class WhiteListManager
    {
        private WhiteListViewModel _whiteList;

        /// <summary>
        /// Initialise une nouvelle instance du gestionnaire de whitelist.
        /// </summary>
        /// <param name="whiteList">Instance initiale du modèle de whitelist.</param>
        public WhiteListManager(WhiteListViewModel whiteList)
        {
            _whiteList = whiteList;
        }

        /// <summary>
        /// Remplace entièrement la whitelist par une nouvelle liste d'IPs et de domaines.
        /// S'éxecute uniquement si la whitelist est vide.
        /// </summary>
        /// <param name="ips">Liste des adresses IP à ajouter.</param>
        /// <param name="domains">Liste des domaines à ajouter.</param>
        public void RefreshWhiteListIfEmpty(List<string> ips, List<string> domains)
        {
            if (!_whiteList.IPv4.Any() && !_whiteList.IPv6.Any() && !_whiteList.Domains.Any())
            {
                RefreshWhiteList(ips, domains);
            }
        }

        /// <summary>
        /// Remplace entièrement la whitelist par une nouvelle liste d'IPs et de domaines.
        /// </summary>
        /// <param name="ips">Liste des adresses IP (IPv4 et/ou IPv6).</param>
        /// <param name="domains">Liste des domaines.</param>
        public void RefreshWhiteList(List<string> ips, List<string> domains)
        {
            _whiteList = CreateWhiteList(domains, ips);
        }

        /// <summary>
        /// Met à jour uniquement les listes d'adresses IP (IPv4 et IPv6).
        /// </summary>
        /// <param name="ips">Liste des adresses IP à traiter.</param>
        public void RefreshIpList(List<string> ips)
        {
            (_whiteList.IPv4, _whiteList.IPv6) = CreateIps(ips);
        }

        /// <summary>
        /// Met à jour uniquement la liste des domaines.
        /// </summary>
        /// <param name="domain">Liste des domaines autorisés.</param>
        public void RefreshDomainList(List<string> domain)
        {
            _whiteList.Domains = domain;
        }

        /// <summary>
        /// Supprime toutes les entrées de la whitelist (IPv4, IPv6 et domaines).
        /// </summary>
        public void ClearWhitelist()
        {
            _whiteList.IPv4.Clear();
            _whiteList.IPv6.Clear();
            _whiteList.Domains.Clear();
        }

        /// <summary>
        /// Indique si la whitelist est complètement vide.
        /// </summary>
        /// <returns>
        /// <c>true</c> si aucune IPv4, IPv6 ou domaine n'est présent ; sinon <c>false</c>.
        /// </returns>
        public bool IsEmpty()
        {
            return !_whiteList.IPv4.Any() && !_whiteList.IPv6.Any() && !_whiteList.Domains.Any();
        }

        /// <summary>
        /// Retourne l'instance actuelle du modèle de whitelist.
        /// </summary>
        /// <returns>Le modèle <see cref="WhiteListViewModel"/> courant.</returns>
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
