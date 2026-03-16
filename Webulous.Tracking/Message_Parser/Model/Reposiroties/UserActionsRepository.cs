using Dapper;
using Message_Parser.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Message_Parser.Model.Reposiroties
{
    internal class UserActionsRepository : BaseRepository
    {
        public bool Insert(UserAction userAction)
        {
            int rows = _connection.Execute(
                "INSERT INTO User_Actions (Time, Page_Id, Action_Type, Action_Parameter) VALUES (@Time, @PageId, @ActionType, @ActionParameter)",
                userAction);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<UserAction> userAction, IDbTransaction? transaction)
        {
            return BulkInsertInternal(userAction, BatchInsert, transaction);
        }

        private async Task<int> BatchInsert(List<UserAction> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Time{i}, @PageId{i}, @ActionType{i}, @ActionParameter{i}),");

                parameters.Add($"Time{i}", batch[i].Time);
                parameters.Add($"PageId{i}", batch[i].PageId);
                parameters.Add($"ActionType{i}", batch[i].ActionType);
                parameters.Add($"ActionParameter{i}", batch[i].ActionParameter);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO User_Actions (Time, Page_Id, Action_Type, Action_Parameter) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
