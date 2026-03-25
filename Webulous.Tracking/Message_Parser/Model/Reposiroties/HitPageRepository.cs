using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;
using System.Drawing;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Message_Parser.Model.Reposiroties
{
    internal class HitPageRepository : BaseRepository
    {
        public HitPageRepository(MySqlConnection connection)
        : base(connection)
        {
        }
        public bool Insert(HitPage hitpage)
        {
            int rows = _connection.Execute(
                "INSERT INTO Hit_Page (Id, Time, Session_Pk, Url, Referrer) VALUES (@Id, @Time, @SessionPk, @Url, @Referrer)",
                hitpage);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<HitPage> hitPages, IDbTransaction? transaction)
        {
            return BulkInsertInternal(hitPages, BatchInsert, transaction);
        }

        public async Task<bool> Contains(int id)
        {
            return _connection.Execute("SELECT 1 FROM Hit_Page WHERE id = (@Id)", new { Id = id }) == 1;
        }

        public int? GetLastId()
        {
            return _connection.QuerySingle<int?>("SELECT MAX(id) FROM Hit_Page");
        }

        public List<int> GetLastHitPageOfSessionOredered(List<Session> sessions)
        {
            var sessionsId = sessions.Select(s => s.Id).ToList();

            var sql = """
                SELECT hp.id
                FROM Hit_Page hp
                JOIN (
                    SELECT session_pk, MAX(time) AS max_time
                    FROM Hit_Page
                    WHERE session_pk IN @SessionsId
                    GROUP BY session_pk
                ) last_hp
                ON hp.session_pk = last_hp.session_pk
                AND hp.time = last_hp.max_time
                ORDER BY hp.session_pk
                """;

            return _connection.Query<int>(sql, new { SessionsId = sessionsId }).ToList();
        }

        public List<KeyValuePair<Session, int>> GetSessionsAfterDateWithLastHitpage(DateTime date)
        {
            var sql = """
                SELECT s.Id, s.Session_Id, s.Site, s.User_Id, s.User_Ip, s.Language_Browser, s.User_Agent, s.Session_Start, s.Session_End, hp.Id AS HitPageId
                FROM Hit_Page hp
                    JOIN (
                        SELECT session_pk, MAX(time) AS max_time
                        FROM Hit_Page
                        WHERE session_pk IN ( SELECT Id FROM Sessions WHERE Session_Start >= (@Date))
                        GROUP BY session_pk
                    ) last_hp
                        ON hp.session_pk = last_hp.session_pk
                        AND hp.time = last_hp.max_time
                    JOIN Sessions s ON s.Id = hp.session_pk;
                """;

            var dico = _connection.Query(sql, date)
                        .Select(s => new KeyValuePair<Session, int>(
                            new Session { Id = s.Id, 
                                SessionId = s.Session_Id, 
                                Site = s.Site, 
                                UserId = s.User_Id, 
                                UserIp = s.User_Ip, 
                                LanguageBrowser = s.Language_Browser, 
                                UserAgent = s.User_Agent, 
                                SessionStart = s.Session_Start, 
                                SessionEnd = s.Session_End },
                            s.HitPageId
                            )).ToList();

            return dico;
        }

        private async Task<int> BatchInsert(List<HitPage> batch, IDbTransaction? transaction)
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

            var sql = $"INSERT INTO Hit_Page (Id, Time, Session_Pk, Url, Referrer) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
