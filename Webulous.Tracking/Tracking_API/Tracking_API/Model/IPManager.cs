using System.Collections;
using System.Collections.Generic;

namespace Tracking_API.Model
{

    /// <summary>
    /// Gère une liste blanche (whitelist) d'adresses IP et leur persistance dans un fichier via FileManager.
    /// </summary>
    public class IPManager
    {
        private readonly List<IPSubnet> _whiteList = new List<IPSubnet>();
        private readonly FileManager _fileManager;

        /// <summary>
        /// Initialise l'IPManager et charge la whitelist depuis le fichier spécifié.
        /// </summary>
        /// <param name="whiteListPath">Chemin du fichier contenant la whitelist</param>
        public IPManager(string whiteListPath)
        {
            _fileManager = new FileManager(whiteListPath);

            try
            {
                AddIpsToSafeList(_fileManager.ReadFile());
            } catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        /// <summary>
        /// Ajoute une IPSubnet à la whitelist si elle n'y est pas déjà.
        /// Sauvegarde la liste mise à jour dans le fichier.
        /// </summary>
        /// <param name="ip">IP à ajouter</param>
        public void AddIpToSafeList(string ip)
        {
            if (!_whiteList.Any(x => x.GetIp().Equals(ip)))
            {
                _whiteList.Add(new IPSubnet(ip));
                SaveToFile();
            }
        }

        /// <summary>
        /// Ajoute plusieurs IPSsubnets à la whitelist.
        /// Appelle AddIpToSafeList pour chaque élément.
        /// </summary>
        /// <param name="ips">Tableau d'IPs à ajouter</param>
        public void AddIpsToSafeList(string[] ips)
        {
            foreach (var ip in ips)
            {
                AddIpToSafeList(ip);
            }
        }

        /// <summary>
        /// Supprime une IPSubnet de la whitelist.
        /// Rajoute auomatiquement un range à l'ip si pas précisé. IPv4 (/32) et IPv6 (/128).
        /// Sauvegarde la liste mise à jour dans le fichier.
        /// </summary>
        /// <param name="ip">IP ou subnet à supprimer</param>
        public void RemoveIpToSafeList(string ip)
        {
            var parts = ip.Split('/');
            if (parts.Length == 1)
            {
                if(ip.Contains(":"))
                {
                    _whiteList.RemoveAll(subnet => subnet.GetIp() == String.Concat(ip, "/128"));
                } else
                {
                    _whiteList.RemoveAll(subnet => subnet.GetIp() == String.Concat(ip, "/32"));
                }
                SaveToFile();
            } else
            {
                _whiteList.RemoveAll(subnet => subnet.GetIp() == ip);
                SaveToFile();
            }  
        }

        /// Vérifie si une IP donnée appartient à la whitelist.
        /// </summary>
        /// <param name="ip">IP à vérifier</param>
        /// <returns>true si l'IP est dans la whitelist, false sinon</returns>
        public bool IsInSafeList(string ip)
        {
            return _whiteList.Any(subnet => subnet.Contains(ip));
        }

        /// <summary>
        /// Retourne toutes les IPs présents dans la whitelist.
        /// </summary>
        /// <returns>Tableau de chaînes représentant chaque IP</returns>
        public string[] GetSafeList()
        {
            return _whiteList.Select(ip => ip.GetIp()).ToArray();
        }

        private void SaveToFile()
        {
            List<string> list = _whiteList.Select(IpSub => IpSub.GetIp()).ToList();
            
            _fileManager.OverwriteFromList(list);
        }
    }
}
