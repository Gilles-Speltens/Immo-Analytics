using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Message_Parser.Model.Reposiroties
{
    internal class SessionsRepository : BaseRepository
    {
        public SessionsRepository(MySqlConnection connection)
        : base(connection)
        {
        }
        public bool Insert(Session session)
        {
            int rows = _connection.Execute(
                "INSERT INTO Sessions (Id, UserId, Language_Browser, User_Agent, Begin, End) VALUES (@Id, @UserId, @LanguageBrowser, @UserAgent, @Begin, @End)",
                session);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<Session> sessions, IDbTransaction? transaction)
        {
            return BulkInsertInternal(sessions, BatchInsert, transaction);
        }

        public async Task<List<Session>> GetAll()
        {
            var sessions = await _connection.QueryAsync<Session>("SELECT * FROM Sessions");
            return sessions.ToList();
        }

        public async Task<bool> Contains(int id)
        {
            return _connection.Execute("SELECT 1 FROM Session WHERE id = (@Id)", new { Id = id }) == 1;
        }

        private async Task<int> BatchInsert(List<Session> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Id{i}, @UserId{i}, @LanguageBrowser{i}, @UserAgent{i}, @Begin{i}, @End{i}),");

                parameters.Add($"Id{i}", batch[i].Id);
                parameters.Add($"UserId{i}", batch[i].UserId);
                parameters.Add($"LanguageBrowser{i}", batch[i].LanguageBrowser);
                parameters.Add($"UserAgent{i}", batch[i].UserAgent);
                parameters.Add($"Begin{i}", batch[i].Begin);
                parameters.Add($"End{i}", batch[i].End);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Sessions (@Id, @UserId, @LanguageBrowser, @UserAgent, @Begin, @End) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
