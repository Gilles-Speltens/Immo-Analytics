using Common;

namespace Tracking_API.Model
{

    /// <summary>
    /// Gère une liste blanche (whitelist) de domaines et leur persistance dans un fichier via <see cref="IFileManager"/>.
    /// </summary>
    public class DomainManager
    {
        private readonly List<string> _whiteList = new List<string>();
        private readonly IFileManager _fileManager;

        /// <summary>
        /// Initialise le <see cref="DomainManager"/> et charge la whitelist depuis le fichier fourni par le <see cref="IFileManager"/>.
        /// </summary>
        /// <param name="fileManager">Gestionnaire de fichier utilisé pour lire et écrire la whitelist.</param>

        public DomainManager(IFileManager fileManager)
        {
            _fileManager = fileManager;

            try
            {
                AddDomainsToSafeList(_fileManager.ReadFile());
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// Ajoute un domaine à la whitelist s'il n'y est pas déjà présent,
        /// puis sauvegarde la liste mise à jour dans le fichier.
        /// </summary>
        /// <param name="domain">Domaine à ajouter.</param>
        public void AddDomainToSafeList(string domain)
        {
            if (!IsInSafeList(domain))
            {
                _whiteList.Add(domain);
                SaveToFile();
            }
        }

        /// <summary>
        /// Ajoute plusieurs domaines à la whitelist.
        /// Chaque domaine est traité individuellement via <see cref="AddDomainToSafeList"/>.
        /// </summary>
        /// <param name="domains">Tableau de domaines à ajouter.</param>
        public void AddDomainsToSafeList(string[] domains)
        {
            foreach (var domain in domains)
            {
                AddDomainToSafeList(domain);
            }
        }

        /// <summary>
        /// Supprime un domaine de la whitelist,
        /// puis sauvegarde la liste mise à jour dans le fichier.
        /// </summary>
        /// <param name="domain">Domaine à supprimer.</param>
        public void RemoveDomainFromSafeList(string domain)
        {
            _whiteList.RemoveAll(d => d.Equals(domain));
            SaveToFile();
        }

        /// <summary>
        /// Vérifie si un domaine est présent dans la whitelist.
        /// </summary>
        /// <param name="domain">Domaine à vérifier.</param>
        /// <returns>
        /// <c>true</c> si le domaine est présent dans la whitelist ; sinon, <c>false</c>.
        /// </returns>
        public bool IsInSafeList(string domain)
        {
            return _whiteList.Any(d => d.Contains(domain));
        }

        /// <summary>
        /// Retourne tous les domaines présents dans la whitelist.
        /// </summary>
        /// <returns>Un tableau de chaînes représentant les domaines autorisés.</returns>
        public string[] GetSafeList()
        {
            return _whiteList.Select(domain => domain).ToArray();
        }

        /// <summary>
        /// Sauvegarde la whitelist actuelle dans le fichier via le <see cref="IFileManager"/>.
        /// </summary>
        private void SaveToFile()
        {
            List<string> list = _whiteList.Select(domain => domain).ToList();

            _fileManager.OverwriteFromList(list);
        }
    }
}