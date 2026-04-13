using Common;
using Tracking_API.Services;

namespace Tracking_API.Infrastructures
{
    /// <summary>
    /// Classe abstraite permettant de gérer une liste blanche (whitelist) générique.
    /// Elle offre des fonctionnalités pour ajouter, supprimer, vérifier et persister
    /// des éléments dans un fichier via un <see cref="IFileManager"/>.
    /// </summary>
    /// <typeparam name="T">
    /// Type des éléments stockés dans la whitelist.
    /// </typeparam>
    public abstract class WhitelistManager<T>
    {
        protected readonly List<T> _whiteList = new();
        protected readonly IFileManager _fileManager;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="WhitelistManager{T}"/>.
        /// Charge automatiquement les éléments existants depuis le fichier associé.
        /// </summary>
        /// <param name="fileManager">
        /// Service responsable de la lecture et de l'écriture des données de la whitelist.
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// Capturée si le fichier n'existe pas lors de la lecture initiale.
        /// </exception>
        protected WhitelistManager(IFileManager fileManager)
        {
            _fileManager = fileManager;

            try
            {
                AddRange(_fileManager.ReadFile());
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Convertit une chaîne de caractères en un objet du type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="input">Représentation textuelle de l'élément.</param>
        /// <returns>Instance convertie du type <typeparamref name="T"/>.</returns>
        protected abstract T Convert(string input);

        /// <summary>
        /// Détermine si un élément de la whitelist correspond exactement à l'entrée fournie.
        /// Utilisé pour éviter les doublons et pour la suppression.
        /// </summary>
        /// <param name="item">Élément de la whitelist.</param>
        /// <param name="input">Valeur à comparer.</param>
        /// <returns>
        /// <c>true</c> si l'élément correspond exactement à l'entrée ; sinon, <c>false</c>.
        /// </returns>
        protected abstract bool Matches(T item, string input);

        /// <summary>
        /// Détermine si un élément de la whitelist contient ou correspond partiellement
        /// à l'entrée fournie. Utilisé pour les vérifications d'appartenance.
        /// </summary>
        /// <param name="item">Élément de la whitelist.</param>
        /// <param name="input">Valeur à vérifier.</param>
        /// <returns>
        /// <c>true</c> si l'entrée est considérée comme autorisée ; sinon, <c>false</c>.
        /// </returns>
        protected abstract bool Contains(T item, string input);

        /// <summary>
        /// Convertit un élément de la whitelist en sa représentation textuelle
        /// pour la persistance dans un fichier.
        /// </summary>
        /// <param name="item">Élément à sérialiser.</param>
        /// <returns>Représentation textuelle de l'élément.</returns>
        protected abstract string Serialize(T item);

        /// <summary>
        /// Ajoute un nouvel élément à la whitelist s'il n'existe pas déjà,
        /// puis sauvegarde la liste dans le fichier.
        /// </summary>
        /// <param name="input">Valeur textuelle de l'élément à ajouter.</param>
        public void Add(string input)
        {
            if (!_whiteList.Any(x => Matches(x, input)))
            {
                _whiteList.Add(Convert(input));
                Save();
            }
        }

        /// <summary>
        /// Ajoute plusieurs éléments à la whitelist.
        /// Chaque élément est traité individuellement afin d'éviter les doublons.
        /// </summary>
        /// <param name="inputs">Collection de valeurs textuelles à ajouter.</param>
        public void AddRange(string[] inputs)
        {
            foreach (var input in inputs)
            {
                Add(input);
            }
        }

        /// <summary>
        /// Supprime tous les éléments correspondant à l'entrée fournie,
        /// puis sauvegarde la liste mise à jour.
        /// </summary>
        /// <param name="input">Valeur textuelle de l'élément à supprimer.</param>
        public void Remove(string input)
        {
            _whiteList.RemoveAll(x => Matches(x, input));
            Save();
        }

        /// <summary>
        /// Vérifie si une entrée est présente dans la whitelist.
        /// </summary>
        /// <param name="input">Valeur à vérifier.</param>
        /// <returns>
        /// <c>true</c> si l'entrée est autorisée ; sinon, <c>false</c>.
        /// </returns>
        public bool IsInSafeList(string input)
        {
            return _whiteList.Any(x => Contains(x, input));
        }

        /// <summary>
        /// Récupère la liste complète des éléments autorisés sous forme de chaînes de caractères.
        /// </summary>
        /// <returns>Tableau contenant les éléments sérialisés de la whitelist.</returns>
        public string[] GetSafeList()
        {
            return _whiteList.Select(Serialize).ToArray();
        }

        /// <summary>
        /// Sauvegarde la whitelist actuelle dans le fichier en écrasant son contenu.
        /// </summary>
        private void Save()
        {
            _fileManager.OverwriteFromList(_whiteList.Select(Serialize).ToList());
        }
    }
}
