using MySqlConnector;
using System.Data;

namespace Message_Parser.Reposiroties
{
    /// <summary>
    /// Classe de base pour les repositories MySQL / MariaDB.
    /// Fournit des fonctionnalités communes comme l’insertion en batch (Bulk Insert).
    /// </summary>
    internal abstract class BaseRepository
    {
        protected readonly int _batchSize = 1000;

        /// <summary>
        /// Méthode interne permettant de réaliser des insertions en masse (bulk insert)
        /// en découpant la liste d’éléments en batches.
        /// </summary>
        /// <typeparam name="T">Type des éléments à insérer.</typeparam>
        /// <param name="items">Liste des éléments à insérer.</param>
        /// <param name="batchInsert">
        /// Fonction qui insère un batch d’éléments en base de données.
        /// Elle reçoit :
        /// - la liste d’éléments du batch
        /// - la transaction éventuelle
        /// </param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre total de lignes insérées.</returns>
        protected async Task<int> BulkInsertInternal<T>(
            List<T> items,
            Func<List<T>, IDbTransaction?, MySqlConnection, Task<int>> batchInsert,
            IDbTransaction? transaction, 
            MySqlConnection connection)
        {
            if (!items.Any()) return 0;

            int total = 0;

            for (int i = 0; i < items.Count; i += _batchSize)
            {
                var batch = items.Skip(i).Take(_batchSize).ToList();
                total += await batchInsert(batch, transaction, connection);
            }

            return total;
        }
    }
}
