using Common;
using Message_Parser.Entities;
using Message_Parser.Model.Reposiroties;
using MySqlConnector;

namespace Message_Parser.Model
{
    internal class UnitOfWork
    {
        private MySqlConnection _db;
        private SessionsRepository _sessionRepo;
        private UsersRepository _userRepo;
        private HitPageRepository _hitpageRepo;
        private UserActionsRepository _userActionsRepo;
        private SiteRepository _websiteRepo;

        public UnitOfWork()
        {
            _db = DBConnection.Instance;
            _sessionRepo = new SessionsRepository(_db);
            _userRepo = new UsersRepository(_db);
            _hitpageRepo = new HitPageRepository(_db);
            _userActionsRepo = new UserActionsRepository(_db);
            _websiteRepo = new SiteRepository(_db);
        }

        //public async Task<bool> bulkInsertLogs(List<RequestLogDto> logs)
        //{
        //    _db.Open();
        //    foreach (RequestLogDto log in logs)
        //    {
        //        var userId = log.UserId;
        //        var sessionId = log.SessionId;
        //        HitPage hitPage = new HitPage { Time = log.Date, Url = log.Url, Referrer = log.UrlReferrer, LanguageBrowser = log.LanguageBrowser, Session = log.SessionId, Website =  };
        //    }
        //}
    }
}
