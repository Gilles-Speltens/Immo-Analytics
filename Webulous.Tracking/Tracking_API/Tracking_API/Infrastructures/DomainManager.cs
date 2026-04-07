using Common;
using System.Text.RegularExpressions;
using Tracking_API.Services;

namespace Tracking_API.Infrastructures
{

    /// <summary>
    /// Gère une liste blanche (whitelist) de domaines et leur persistance dans un fichier via <see cref="IFileManager"/>.
    /// </summary>
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