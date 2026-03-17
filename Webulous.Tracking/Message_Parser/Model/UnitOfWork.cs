using Common;
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
        private WebsiteRepository _websiteRepo;

        public UnitOfWork()
        {
            _db = DBConnection.Instance;
            _sessionRepo = new SessionsRepository(_db);
            _userRepo = new UsersRepository(_db);
            _hitpageRepo = new HitPageRepository(_db);
            _userActionsRepo = new UserActionsRepository(_db);
            _websiteRepo = new WebsiteRepository(_db);
        }

        //public async Task<bool> bulkInsertLogs(List<RequestLogDto> logs)
        //{
        //    foreach (RequestLogDto log in logs) 
        //    {

        //    }
        //}
    }
}
