using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;
using System.Text;

namespace Message_Parser.Model.Reposiroties
{
    internal class WebsiteRepository : BaseRepository
    {
        public WebsiteRepository(MySqlConnection connection)
        : base(connection)
        {
        }
        public bool Insert(Website website)
        {
            int rows = _connection.Execute(
                "INSERT INTO Web_Site (Domain, Date_When_Added, Certify) VALUES (@Domain, @DateWhenAdded, @Certify)",
                website);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<Website> websites, IDbTransaction? transaction)
        {
            return BulkInsertInternal(websites, BatchInsert, transaction);
        }

        public async Task<bool> Contains(string domain)
        {
            return _connection.Execute("SELECT 1 FROM Web_Site WHERE domain = (@Domain)", new { Domain = domain }) == 1;
        }

        private async Task<int> BatchInsert(List<Website> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Domain{i}, @DateWhenAdded{i}, @Certify{i}),");

                parameters.Add($"Domain{i}", batch[i].Domain);
                parameters.Add($"DateWhenAdded{i}", batch[i].DateWhenAdded);
                parameters.Add($"Certify{i}", batch[i].Certify);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Web_Site (Domain, Date_When_Added, Certify) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
