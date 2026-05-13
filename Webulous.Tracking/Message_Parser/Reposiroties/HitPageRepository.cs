using Dapper;
using Message_Parser.Data;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;
using System.Drawing;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Message_Parser.Reposiroties
{
    /// <summary>
    /// Repository responsable de l’accès aux données de la table Hit_Page.
    /// Permet l’insertion simple, l’insertion en batch, la récupération d’informations
    /// liées aux hit pages et aux sessions.
    /// </summary>
    public class HitPageRepository : BaseRepository
    {
        /// <summary>
        /// Insère une HitPage en base de données.
        /// </summary>
        /// <param name="hitpage">Objet HitPage à insérer.</param>
        /// <returns>True si l’insertion a réussi.</returns>
        public bool Insert(HitPage hitpage, MySqlConnection connection)
        {
            int rows = connection.Execute(
                "INSERT INTO hit_page (id, time, session_pk, url, referrer) VALUES (@Id, @Time, @SessionPk, @Url, @Referrer)",
                hitpage);

            return rows == 1;
        }

        /// <summary>
        /// Insère une liste de HitPage en base de données en utilisant des batchs.
        /// </summary>
        /// <param name="hitPages">Liste des HitPages.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre total de lignes insérées.</returns>
        public Task<int> BulkInsert(List<HitPage> hitPages, IDbTransaction? transaction, MySqlConnection connection)
        {
            return BulkInsertInternal(hitPages, BatchInsert, transaction, connection);
        }

        /// <summary>
        /// Vérifie si une HitPage avec un Id donné existe en base.
        /// </summary>
        /// <param name="id">Identifiant de la HitPage.</param>
        /// <returns>True si elle existe.</returns>
        public async Task<bool> Contains(int id, MySqlConnection connection)
        {
            return connection.Execute("SELECT 1 FROM hit_page WHERE id = (@Id)", new { Id = id }) == 1;
        }

        /// <summary>
        /// Récupère le dernier Id (maximum) présent dans la table Hit_Page.
        /// </summary>
        /// <returns>Le dernier Id ou null si la table est vide.</returns>
        public int? GetLastId(MySqlConnection connection)
        {
            return connection.QuerySingle<int?>("SELECT MAX(id) FROM hit_page");
        }

        /// <summary>
        /// Récupère les sessions après une certaine date avec leur dernière HitPage.
        /// </summary>
        /// <param name="date">Date minimale de début de session.</param>
        /// <returns>
        /// Liste de couples :
        /// - Session
        /// - Id de la dernière HitPage
        /// </returns>
        public List<KeyValuePair<Session, long>> GetSessionsAfterDateWithLastHitpage(DateTime date, MySqlConnection connection)
        {
            var sql = """
                SELECT s.id, s.session_id, s.site, s.user_id, s.user_ip, s.language_browser, s.user_agent, s.session_start, s.session_end, hp.id AS HitPageId
                FROM sessions s
                LEFT JOIN hit_page hp 
                    ON hp.id = (
                        SELECT hp2.id
                        FROM hit_page hp2
                        WHERE hp2.session_pk = s.id
                        ORDER BY hp2.time DESC
                        LIMIT 1
                    )
                WHERE s.session_start >= @Date;
                """;

            var rows = connection.Query<SessionWithHitPage>(
                sql,
                new { Date = date });

            var dico = rows
                .Select(s => new KeyValuePair<Session, long>(
                    new Session
                    {
                        Id = s.Id,
                        SessionId = s.Session_Id,
                        Site = s.Site,
                        UserId = s.User_Id,
                        UserIp = s.User_Ip,
                        LanguageBrowser = s.Language_Browser,
                        UserAgent = s.User_Agent,
                        SessionStart = s.Session_Start,
                        SessionEnd = s.Session_End
                    },
                    s.HitPageId
                ))
                .ToList();

            return dico;
        }

        /// <summary>
        /// Méthode interne permettant d’insérer un batch de HitPages
        /// en construisant une requête SQL multi-values.
        /// </summary>
        /// <param name="batch">Batch de HitPages.</param>
        /// <param name="transaction">Transaction SQL optionnelle.</param>
        /// <returns>Nombre de lignes insérées.</returns>
        private async Task<int> BatchInsert(List<HitPage> batch, IDbTransaction? transaction, MySqlConnection connection)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Id{i}, @Time{i}, @SessionId{i}, @Url{i}, @Referrer{i}),");

                parameters.Add($"Id{i}", batch[i].Id);
                parameters.Add($"Time{i}", batch[i].Time);
                parameters.Add($"SessionId{i}", batch[i].SessionPk);
                parameters.Add($"Url{i}", batch[i].Url);
                parameters.Add($"Referrer{i}", batch[i].Referrer);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO hit_page (id, time, session_pk, url, referrer) VALUES {sqlValues}";

            return await connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
