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
                "INSERT INTO Sessions (Id, SessionId, Site, UserId, UserIp, Language_Browser, User_Agent, SessionStart, SessionEnd) VALUES (@Id, @SessionId, @Site, @UserId, @UserIp, @LanguageBrowser, @UserAgent, @SessionStart, @SessionEnd)",
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

        public List<Session> GetAllAfterDateOrdered(DateTime date)
        {
            var sessions = _connection.Query<Session>("SELECT * FROM Session WHERE Session_Start >= (@Date) ORDER BY Session_Start", date);
            return sessions.ToList();
        }

        private async Task<int> BatchInsert(List<Session> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Id{i}, @SessionId{i}, @Site{i}, @UserId{i}, @UserIp{i}, @LanguageBrowser{i}, @UserAgent{i}, @SessionStart{i}, @SessionEnd{i}),");

                parameters.Add($"Id{i}", batch[i].Id);
                parameters.Add($"SessionId{i}", batch[i].SessionId);
                parameters.Add($"Site{i}", batch[i].Site);
                parameters.Add($"UserId{i}", batch[i].UserId);
                parameters.Add($"UserIp{i}", batch[i].UserIp);
                parameters.Add($"LanguageBrowser{i}", batch[i].LanguageBrowser);
                parameters.Add($"UserAgent{i}", batch[i].UserAgent);
                parameters.Add($"SessionStart{i}", batch[i].SessionStart);
                parameters.Add($"SessionEnd{i}", batch[i].SessionEnd);
            }

            sqlValues.Length--;

            var sql = $"""
                INSERT INTO Sessions (Session_Id, Site, User_Id, User_Ip, Language_Browser, User_Agent, Session_Start, Session_End)
                VALUES {sqlValues}
                """;

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
