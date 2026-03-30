using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Message_Parser.Model.Reposiroties
{
    /// <summary>
    /// Repository responsable de l’accès aux données de la table User_Actions.
    /// Permet l’insertion simple et en batch des actions utilisateurs.
    /// </summary>
    internal class UserActionsRepository : BaseRepository
    {
        /// <summary>
        /// Insère une action utilisateur en base de données.
        /// </summary>
        /// <param name="userAction">Action utilisateur à insérer.</param>
        /// <returns>True si l’insertion a réussi.</returns>
        public bool Insert(UserAction userAction, MySqlConnection connection)
        {
            int rows = connection.Execute(
                "INSERT INTO User_Actions (Time, Page_Id, Action_Type, Action_Parameter) VALUES (@Time, @PageId, @ActionType, @ActionParameter)",
                userAction);

            return rows == 1;
        }

        /// <summary>
        /// Insère une liste d’actions utilisateur en base de données en utilisant des batchs.
        /// </summary>
        /// <param name="userAction">Liste des actions utilisateur.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre total de lignes insérées.</returns>
        public Task<int> BulkInsert(List<UserAction> userAction, IDbTransaction? transaction, MySqlConnection connection)
        {
            return BulkInsertInternal(userAction, BatchInsert, transaction, connection);
        }

        /// <summary>
        /// Méthode interne permettant d’insérer un batch d’actions utilisateur
        /// en construisant une requête SQL multi-values.
        /// </summary>
        /// <param name="batch">Batch d’actions utilisateur.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre de lignes insérées.</returns>
        private async Task<int> BatchInsert(List<UserAction> batch, IDbTransaction? transaction, MySqlConnection connection)
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

            return await connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
