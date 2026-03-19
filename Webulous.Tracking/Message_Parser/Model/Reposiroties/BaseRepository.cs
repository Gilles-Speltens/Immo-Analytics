using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

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

        protected async Task<List<int>> BulkUpsertInternal<T>(
            List<T> items,
            Func<List<T>, IDbTransaction?, Task<List<int>>> batchUpsert,
            IDbTransaction? transaction)
        {
            List<int> idAdded = new List<int>();

            if (!items.Any()) return idAdded;

            for (int i = 0; i < items.Count; i += _batchSize)
            {
                var batch = items.Skip(i).Take(_batchSize).ToList();
                idAdded.AddRange(await batchUpsert(batch, transaction));
            }

            return idAdded;
        }
    }
}
