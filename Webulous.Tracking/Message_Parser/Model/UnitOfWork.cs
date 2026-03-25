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

        private LogProcessingService _logProcessingService;
        public UnitOfWork(int sessionExpirationTime)
        {
            _db = DBConnection.Instance;
            _sessionRepo = new SessionsRepository(_db);
            _hitpageRepo = new HitPageRepository(_db);
            _userActionsRepo = new UserActionsRepository(_db);
            _siteRepo = new SiteRepository(_db);

            var existingDomains = new HashSet<string>(_siteRepo.GetAllDomain());
            var tempSessions = _hitpageRepo.GetSessionsAfterDateWithLastHitpage(DateTime.UtcNow.AddMinutes(-(sessionExpirationTime)));
            var lastHitPageId = _hitpageRepo.GetLastId() ?? 0;
            var lastSessionId = _sessionRepo.GetLastId() ?? 0;

            var ongoingSessions = new Dictionary<(string sessionId, string domain), (Session session, int lastHitPageId)>();

            foreach (var sessionHit in tempSessions)
            {
                var session = sessionHit.Key;
                
                if (session.SessionId != null)
                {
                    var hitpage = sessionHit.Value;
                    ongoingSessions.Add((session.SessionId, session.Site), (session, sessionHit.Value));
                }
            }

            _logProcessingService = new LogProcessingService(existingDomains, ongoingSessions, lastHitPageId, lastSessionId);
        }

        public async Task<bool> bulkInsertLogs(List<RequestLogDto> logs)
        {
            _logProcessingService.ProcessLogs(logs);
            var newSites = _logProcessingService.GetSites();
            var newSessions = _logProcessingService.GetSessions();
            var newHitPages = _logProcessingService.GetHitPages();
            var userActions = _logProcessingService.GetUserActions();

            using (_db)
            {
                _db.Open();

                using (var transaction = _db.BeginTransaction())
                {
                    try
                    {
                        await _siteRepo.BulkInsert(newSites, transaction);
                        await _sessionRepo.BulkInsert(newSessions, transaction);
                        await _hitpageRepo.BulkInsert(newHitPages, transaction);
                        await _userActionsRepo.BulkInsert(userActions, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex.Message.ToString());
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}
