using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Message_Parser.Reposiroties
{
    /// <summary>
    /// Repository responsable de l’accès aux données de la table Sessions.
    /// Gère les opérations CRUD et les insertions en batch.
    /// </summary>
    internal class SessionsRepository : BaseRepository
    {
        /// <summary>
        /// Insère une session en base de données.
        /// </summary>
        /// <param name="session">Session à insérer.</param>
        /// <returns>True si l’insertion a réussi.</returns>
        public bool Insert(Session session, MySqlConnection connection)
        {
            int rows = connection.Execute(
                "INSERT INTO Sessions (Id, SessionId, Site, UserId, UserIp, Language_Browser, User_Agent, SessionStart, SessionEnd) VALUES (@Id, @SessionId, @Site, @UserId, @UserIp, @LanguageBrowser, @UserAgent, @SessionStart, @SessionEnd)",
                session);

            return rows == 1;
        }

        /// <summary>
        /// Insère une liste de sessions en base de données en utilisant des batchs.
        /// </summary>
        /// <param name="sessions">Liste des sessions.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre total de lignes insérées.</returns>
        public Task<int> BulkInsert(List<Session> sessions, IDbTransaction? transaction, MySqlConnection connection)
        {
            return BulkInsertInternal(sessions, BatchInsert, transaction, connection);
        }

        /// <summary>
        /// Récupère toutes les sessions présentes en base de données.
        /// </summary>
        /// <returns>Liste de toutes les sessions.</returns>
        public async Task<List<Session>> GetAll(MySqlConnection connection)
        {
            var sessions = await connection.QueryAsync<Session>("SELECT * FROM Sessions");
            return sessions.ToList();
        }

        /// <summary>
        /// Vérifie si une session avec un Id donné existe en base.
        /// </summary>
        /// <param name="id">Identifiant de la session.</param>
        /// <returns>True si la session existe.</returns>
        public async Task<bool> Contains(int id, MySqlConnection connection)
        {
            return connection.Execute("SELECT 1 FROM Sessions WHERE id = (@Id)", new { Id = id }) == 1;
        }

        /// <summary>
        /// Met à jour la date de fin de session et l’utilisateur associé.
        /// </summary>
        /// <param name="session">Session contenant les nouvelles valeurs.</param>
        public void UpdateUserIdDateEnd(Session session, MySqlConnection connection)
        {
            var sql = connection.Query<int>("""
                UPDATE Sessions
                SET Session_End = @SessionEnd, User_Id = @UserId
                WHERE Session_Id = @SessionId AND Site = @Site
                """, new { SessionEnd = session.SessionEnd, UserId = session.UserId, SessionId = session.SessionId, Site = session.Site });
        }

        /// <summary>
        /// Récupère le dernier Id (maximum) présent dans la table Sessions.
        /// </summary>
        /// <returns>Le dernier Id ou null si la table est vide.</returns>
        public int? GetLastId(MySqlConnection connection)
        {
            return connection.QuerySingle<int?>("SELECT MAX(id) FROM Sessions");
        }

        /// <summary>
        /// Méthode interne permettant d’insérer un batch de sessions
        /// via une requête SQL multi-values.
        /// </summary>
        /// <param name="batch">Batch de sessions.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre de lignes insérées.</returns>
        private async Task<int> BatchInsert(List<Session> batch, IDbTransaction? transaction, MySqlConnection connection)
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
                INSERT INTO Sessions (Id, Session_Id, Site, User_Id, User_Ip, Language_Browser, User_Agent, Session_Start, Session_End)
                VALUES {sqlValues}
                """;

            return await connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
