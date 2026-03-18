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
        public bool Insert(User user)
        {
            int rows = _connection.Execute(
                "INSERT INTO Users (Ip, Id) VALUES (@Ip, @Id)",
                user);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<User> users, IDbTransaction? transaction)
        {
            return BulkInsertInternal(users, BatchInsert, transaction);
        }

        public async Task<List<User>> GetAll()
        {
            var users = await _connection.QueryAsync<User>("SELECT * FROM Users");
            return users.ToList();
        }

        public async Task<bool> Contains(int ip)
        {
            return _connection.Execute("SELECT 1 FROM Users WHERE ip = (@Ip)", new {Ip = ip}) == 1;
        }

        private async Task<int> BatchInsert(List<User> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Ip{i}, @Id{i}),");
                parameters.Add($"Ip{i}", batch[i].Ip);
                parameters.Add($"Id{i}", batch[i].Id);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Users (Ip, Id) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}