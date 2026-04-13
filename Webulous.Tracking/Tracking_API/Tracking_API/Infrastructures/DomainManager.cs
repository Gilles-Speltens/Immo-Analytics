using Common;
using System.Text.RegularExpressions;
using Tracking_API.Services;

namespace Tracking_API.Infrastructures
{

    /// <summary>
    /// Implémentation concrète de <see cref="WhitelistManager{T}"/> pour la gestion
    /// d'une liste blanche de noms de domaine.
    /// </summary>
    /// <remarks>
    /// Cette classe permet d'ajouter, de supprimer et de vérifier la présence
    /// de domaines autorisés. Les comparaisons sont effectuées sans tenir compte
    /// de la casse (case-insensitive) afin de garantir un comportement cohérent
    /// avec les spécifications des noms de domaine.
    /// </remarks>
    public class DomainManager : WhitelistManager<string>
    {
        public DomainManager(IFileManager fileManager) : base(fileManager) { }

        protected override string Convert(string input)
            => input;

        protected override bool Matches(string item, string input)
            => item.Equals(input, StringComparison.OrdinalIgnoreCase);

        protected override bool Contains(string item, string input)
            => item.Equals(input, StringComparison.OrdinalIgnoreCase);

        protected override string Serialize(string item)
            => item;
    }
}