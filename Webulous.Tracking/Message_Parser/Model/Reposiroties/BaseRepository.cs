using MySqlConnector;
using System.Data;

namespace Message_Parser.Model.Reposiroties
{
    internal abstract class BaseRepository
    {
        protected readonly MySqlConnection _connection;
        protected readonly int _batchSize = 1000;

        public BaseRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        protected async Task<int> BulkInsertInternal<T>(
            List<T> items,
            Func<List<T>, IDbTransaction?, Task<int>> batchInsert,
            IDbTransaction? transaction)
        {
            if (!items.Any()) return 0;

            int total = 0;

            for (int i = 0; i < items.Count; i += _batchSize)
            {
                var batch = items.Skip(i).Take(_batchSize).ToList();
                total += await batchInsert(batch, transaction);
            }

            return total;
        }
    }
}
