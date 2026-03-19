using Common;
using Message_Parser.Entities;
using Message_Parser.Model.Reposiroties;
using MySqlConnector;
using System.Text.RegularExpressions;

namespace Message_Parser.Model
{
    internal class UnitOfWork
    {
        private MySqlConnection _db;
        private SessionsRepository _sessionRepo;
        private HitPageRepository _hitpageRepo;
        private UserActionsRepository _userActionsRepo;
        private SiteRepository _siteRepo;

        public UnitOfWork()
        {
            _db = DBConnection.Instance;
            _sessionRepo = new SessionsRepository(_db);
            _hitpageRepo = new HitPageRepository(_db);
            _userActionsRepo = new UserActionsRepository(_db);
            _siteRepo = new SiteRepository(_db);
        }

        //public async Task<bool> bulkInsertLogs(List<RequestLogDto> logs)
        //{
        //    _db.Open();

        //    List<Site> sites = new List<Site>();
        //    foreach (RequestLogDto log in logs)
        //    {
        //        var domain = Regex.Match(log.Url, @"^(?:https?:\/\/)?([^\/:?#]+)").Groups[1].Value;
        //        var dateWhenAdded = DateTime.UtcNow;
        //        var certify = false;
        //        Site site = new Site { Domain = domain, DateWhenAdded = dateWhenAdded, Certify = certify };

        //        sites.Add(site);
        //    }

            
        //}
    }
}
