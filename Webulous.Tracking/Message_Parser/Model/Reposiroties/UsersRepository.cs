using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;
using System.Text;

namespace Message_Parser.Model.Reposiroties
{
    internal class UsersRepository : BaseRepository
    {
        public UsersRepository(MySqlConnection connection)
        : base(connection)
        {
        }
        public bool Insert(string guid)
        {
            int rows = _connection.Execute(
                "INSERT INTO Users (Id) VALUES (@Id)",
                new { Id = guid });

            return rows == 1;
        }

        public Task<int> BulkInsert(List<User> users, IDbTransaction? transaction)
        {
            return BulkInsertInternal(users, BatchInsert, transaction);
        }

        public async Task<bool> Contains(int guid)
        {
            return _connection.Execute("SELECT 1 FROM Users WHERE id = (@Id)", new {Id = guid}) == 1;
        }

        private async Task<int> BatchInsert(List<User> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Id{i}),");
                parameters.Add($"Id{i}", batch[i].Id);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Users (Id) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}