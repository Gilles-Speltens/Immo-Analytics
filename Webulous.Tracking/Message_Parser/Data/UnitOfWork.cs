using Common;
using Message_Parser.Entities;
using Message_Parser.Reposiroties;
using Message_Parser.Services;
using MySqlConnector;

namespace Message_Parser.Data
{
    public class UnitOfWork
    {
        private DBConnection _connectionManager;
        private SessionsRepository _sessionRepo;
        private HitPageRepository _hitpageRepo;
        private UserActionsRepository _userActionsRepo;
        private SiteRepository _siteRepo;

        private LogProcessingService _logProcessingService;
        public UnitOfWork(int sessionExpirationTime, string connectionString)
        {
            _connectionManager = new DBConnection(connectionString);
            _sessionRepo = new SessionsRepository();
            _hitpageRepo = new HitPageRepository();
            _userActionsRepo = new UserActionsRepository();
            _siteRepo = new SiteRepository();

            MySqlConnection tempConnection = _connectionManager.CreateConnection();
            
            var tempSessions = _hitpageRepo.GetSessionsAfterDateWithLastHitpage(DateTime.UtcNow.AddMinutes(-(sessionExpirationTime)), tempConnection);
            var lastHitPageId = _hitpageRepo.GetLastId(tempConnection) ?? 0;
            var lastSessionId = _sessionRepo.GetLastId(tempConnection) ?? 0;

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

            _logProcessingService = new LogProcessingService(ongoingSessions, lastHitPageId, lastSessionId);
        }

        public async Task<bool> bulkInsertLogs(List<RequestLogDto> logs)
        {
            _logProcessingService.ProcessLogs(logs);
            var newSites = _logProcessingService.GetSites();
            var newSessions = _logProcessingService.GetSessions();
            var newHitPages = _logProcessingService.GetHitPages();
            var userActions = _logProcessingService.GetUserActions();

            var connection = _connectionManager.CreateConnection();
            using (connection)
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await _siteRepo.BulkInsert(newSites, transaction, connection);
                        await _sessionRepo.BulkInsert(newSessions, transaction, connection);
                        await _hitpageRepo.BulkInsert(newHitPages, transaction, connection);
                        await _userActionsRepo.BulkInsert(userActions, transaction, connection);

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
