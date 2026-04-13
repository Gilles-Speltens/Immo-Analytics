using Common;
using Tracking_API.Services;

namespace Tracking_API.Infrastructures
{

    /// <summary>
    /// Implémentation concrète de <see cref="WhitelistManager{T}"/> pour la gestion
    /// d'une liste blanche d'adresses IP et de sous-réseaux.
    /// </summary>
    /// <remarks>
    /// Cette classe utilise le type <see cref="IPSubnet"/> pour représenter les adresses
    /// IP ou plages d'adresses (notation CIDR). Elle permet d'ajouter, de supprimer,
    /// de vérifier et de persister ces informations via un <see cref="IFileManager"/>.
    /// </remarks>
    public class IPManager : WhitelistManager<IPSubnet>
    {
        public IPManager(IFileManager fileManager) : base(fileManager) { }

        protected override IPSubnet Convert(string input)
            => new IPSubnet(input);

        protected override bool Matches(IPSubnet item, string input)
            => item.Matches(input);

        protected override bool Contains(IPSubnet item, string input)
            => item.Contains(input);

        protected override string Serialize(IPSubnet item)
            => item.GetIp();
    }
}
