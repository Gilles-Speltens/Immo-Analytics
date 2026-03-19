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
                "INSERT INTO Sessions (Id, SessionId, SiteId, UserId, UserIp, Language_Browser, User_Agent, SessionStart, SessionEnd) VALUES (@Id, @SessionId, @SiteId, @UserId, @UserIp, @LanguageBrowser, @UserAgent, @SessionStart, @SessionEnd)",
                session);

            return rows == 1;
        }

        public Task<List<int>> BulkUpsert(List<Session> sessions, IDbTransaction? transaction)
        {
            return BulkUpsertInternal(sessions, BatchUpsert, transaction);
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

        private async Task<List<int>> BatchUpsert(List<Session> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@SessionId{i}, @SiteId{i}, @UserId{i}, @UserIp{i}, @LanguageBrowser{i}, @UserAgent{i}, @SessionStart{i}, @SessionEnd{i}),");

                parameters.Add($"SessionId{i}", batch[i].SessionId);
                parameters.Add($"SiteId{i}", batch[i].SiteId);
                parameters.Add($"UserId{i}", batch[i].UserId);
                parameters.Add($"UserIp{i}", batch[i].UserIp);
                parameters.Add($"LanguageBrowser{i}", batch[i].LanguageBrowser);
                parameters.Add($"UserAgent{i}", batch[i].UserAgent);
                parameters.Add($"SessionStart{i}", batch[i].SessionStart);
                parameters.Add($"SessionEnd{i}", batch[i].SessionEnd);
            }

            sqlValues.Length--;

            // !!! Fonctionne uniquement avec MariaDB 10.5 (2020)
            var sql = $"""
                INSERT INTO Sessions (Session_Id, Site_Id, User_Id, User_Ip, Language_Browser, User_Agent, Session_Start, Session_End)
                VALUES {sqlValues}
                ON DUPLICATE KEY UPDATE
                    Language_Browser = Language_Browser
                RETURNING Id;
                """;

            return (await _connection.QueryAsync<int>(sql, parameters, transaction)).ToList();

            // Si non utiliser ceci (plus lent de 20%) : !!!!!!!!! buggé à changer !!!!!!!!!
            //var sql = $"""
            //    INSERT INTO Sessions (Session_Id, Site_Id, User_Id, User_Ip, Language_Browser, User_Agent, Session_Start, Session_End)
            //    VALUES {sqlValues}
            //    ON DUPLICATE KEY UPDATE
            //        Language_Browser = Language_Browser
            //    """;

            //await _connection.QueryAsync<int>(sql, parameters, transaction);

            //var userIp = batch.Select(x => x.UserIp).ToList();

            //var ids = (await _connection.QueryAsync<int>(
            //    "SELECT Id FROM Sessions WHERE User_Ip IN @UserIp",
            //    new { UserIp = userIp },
            //    transaction)).ToList();

            //return ids;
        }
    }
}
