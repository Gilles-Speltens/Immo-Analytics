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
        public bool Insert(Session session)
        {
            int rows = _connection.Execute(
                "INSERT INTO Sessions (Id, UserId, Duration) VALUES (@Id, @UserId, @Duration)",
                session);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<Session> sessions, IDbTransaction? transaction)
        {
            return BulkInsertInternal(sessions, BatchInsert, transaction);
        }

        private async Task<int> BatchInsert(List<Session> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Id{i}, @UserId{i}, @Duration{i}),");

                parameters.Add($"Id{i}", batch[i].Id);
                parameters.Add($"UserId{i}", batch[i].UserId);
                parameters.Add($"Duration{i}", batch[i].Duration);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Sessions (Id, User_Id, Duration) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
