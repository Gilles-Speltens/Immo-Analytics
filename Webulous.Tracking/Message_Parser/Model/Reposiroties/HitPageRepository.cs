using Dapper;
using Message_Parser.Entities;
using System.Data;
using System.Text;

namespace Message_Parser.Model.Reposiroties
{
    internal class HitPageRepository : BaseRepository
    {
        public bool Insert(HitPage hitpage)
        {
            int rows = _connection.Execute(
                "INSERT INTO Hit_Page (Time, Url, Referrer, Language_Browser, Session_Id, Website) VALUES (@Time, @Url, @Referrer, @LanguageBrowser, @SessionId, @Website)",
                hitpage);

            return rows == 1;
        }

        public Task<int> BulkInsert(List<HitPage> hitPages, IDbTransaction? transaction)
        {
            return BulkInsertInternal(hitPages, BatchInsert, transaction);
        }

        private async Task<int> BatchInsert(List<HitPage> batch, IDbTransaction? transaction)
        {
            var sqlValues = new StringBuilder();
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                sqlValues.Append($"(@Time{i}, @Url{i}, @Referrer{i}, @LanguageBrowser{i}, @SessionId{i}, @Website{i}),");

                parameters.Add($"Time{i}", batch[i].Time);
                parameters.Add($"Url{i}", batch[i].Url);
                parameters.Add($"Referrer{i}", batch[i].Referrer);
                parameters.Add($"LanguageBrowser{i}", batch[i].LanguageBrowser);
                parameters.Add($"SessionId{i}", batch[i].SessionId);
                parameters.Add($"Website{i}", batch[i].Website);
            }

            sqlValues.Length--;

            var sql = $"INSERT INTO Hit_Page (Time, Url, Referrer, Language_Browser, Session_Id, Website) VALUES {sqlValues}";

            return await _connection.ExecuteAsync(sql, parameters, transaction);
        }
    }
}
