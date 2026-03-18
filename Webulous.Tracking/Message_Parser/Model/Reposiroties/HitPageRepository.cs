using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;
using System.Text;

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
                "INSERT INTO Hit_Page (Time, Session_Id, Url, Referrer, Site_Id) VALUES (@Time, @SessionId, @Url, @Referrer, @SiteId)",
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

        private async Task<int> BatchInsert(List<HitPage> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Time{i}, @SessionId{i}, @Url{i}, @Referrer{i}, @LanguageBrowser{i}, @SiteId{i}),");

                parameters.Add($"Time{i}", batch[i].Time);
                parameters.Add($"SessionId{i}", batch[i].SessionId);
                parameters.Add($"Url{i}", batch[i].Url);
                parameters.Add($"Referrer{i}", batch[i].Referrer);
                parameters.Add($"Website{i}", batch[i].SiteId);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Hit_Page (Time, Session_Id, Url, Referrer, Site_Id) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
