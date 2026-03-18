using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;
using System.Text;

namespace Message_Parser.Model.Reposiroties
{
    internal class SiteRepository : BaseRepository
    {
        public SiteRepository(MySqlConnection connection)
        : base(connection)
        {
        }
        public bool Insert(Site site)
        {
            int rows = _connection.Execute(
                "INSERT INTO Site (Id, Domain, Date_When_Added, Certify) VALUES (@Id, @Domain, @DateWhenAdded, @Certify)",
                site);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<Site> sites, IDbTransaction? transaction)
        {
            return BulkInsertInternal(sites, BatchInsert, transaction);
        }

        public async Task<bool> Contains(string id)
        {
            return _connection.Execute("SELECT 1 FROM Site WHERE id = (@Id)", new { Id = id }) == 1;
        }

        private async Task<int> BatchInsert(List<Site> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Id{i}, @Domain{i}, @DateWhenAdded{i}, @Certify{i}),");

                parameters.Add($"Id{i}", batch[i].Id);
                parameters.Add($"Domain{i}", batch[i].Domain);
                parameters.Add($"DateWhenAdded{i}", batch[i].DateWhenAdded);
                parameters.Add($"Certify{i}", batch[i].Certify);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Site (Id, Domain, Date_When_Added, Certify) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
