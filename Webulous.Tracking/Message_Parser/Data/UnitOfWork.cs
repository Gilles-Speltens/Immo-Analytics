using Common;
using Message_Parser.Entities;
using Message_Parser.Reposiroties;
using Message_Parser.Services;
using MySqlConnector;

namespace Message_Parser.Data
{
    /// <summary>
    /// Implémente le pattern <c>Unit of Work</c> pour coordonner les opérations
    /// de traitement et de persistance des logs dans la base de données.
    /// </summary>
    /// <remarks>
    /// Cette classe centralise :
    /// <list type="bullet">
    /// <item>
    /// <description>La gestion de la connexion à la base de données MySQL.</description>
    /// </item>
    /// <item>
    /// <description>La coordination des différents repositories.</description>
    /// </item>
    /// <item>
    /// <description>Le traitement des logs via <see cref="LogProcessingService"/>.</description>
    /// </item>
    /// <item>
    /// <description>L'exécution atomique des insertions grâce à une transaction.</description>
    /// </item>
    /// </list>
    /// Le pattern garantit que toutes les opérations de persistance sont exécutées
    /// comme une seule unité de travail, assurant ainsi la cohérence des données.
    /// </remarks>
    public class UnitOfWork
    {
        private DBConnection _connectionManager;
        private SessionsRepository _sessionRepo;
        private HitPageRepository _hitpageRepo;
        private UserActionsRepository _userActionsRepo;
        private SiteRepository _siteRepo;
        private MonitoringRepository _fileMonitoring;

        private LogProcessingService _logProcessingService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="UnitOfWork"/>.
        /// </summary>
        /// <param name="sessionExpirationTime">
        /// Durée d'expiration des sessions en minutes. Les sessions actives
        /// après cette période sont considérées comme en cours.
        /// </param>
        /// <param name="connectionString">
        /// Chaîne de connexion à la base de données MySQL.
        /// </param>
        /// <remarks>
        /// Lors de l'initialisation, la classe :
        /// <list type="number">
        /// <item>
        /// <description>Récupère les sessions en cours depuis la base de données.</description>
        /// </item>
        /// <item>
        /// <description>Détermine les derniers identifiants utilisés pour les sessions et les pages visitées.</description>
        /// </item>
        /// <item>
        /// <description>Initialise le <see cref="LogProcessingService"/> avec ces informations.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public UnitOfWork(int sessionExpirationTime, string connectionString)
        {
            _connectionManager = new DBConnection(connectionString);
            _sessionRepo = new SessionsRepository();
            _hitpageRepo = new HitPageRepository();
            _userActionsRepo = new UserActionsRepository();
            _siteRepo = new SiteRepository();
            _fileMonitoring = new MonitoringRepository();

            MySqlConnection tempConnection = _connectionManager.CreateConnection();
            
            var tempSessions = _hitpageRepo.GetSessionsAfterDateWithLastHitpage(DateTime.UtcNow.AddMinutes(-(sessionExpirationTime)), tempConnection);
            var lastHitPageId = _hitpageRepo.GetLastId(tempConnection) ?? 0;
            var lastSessionId = _sessionRepo.GetLastId(tempConnection) ?? 0;

            foreach(var temp in  tempSessions)
            {
                Console.WriteLine(temp.Key.Id.ToString() + " " + temp.Value);
            }

            var ongoingSessions = new Dictionary<(string sessionId, string domain), (Session session, long lastHitPageId)>();

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

        /// <summary>
        /// Traite une collection de logs et effectue leur insertion en base de données
        /// de manière transactionnelle.
        /// </summary>
        /// <param name="logs">
        /// Liste des objets <see cref="RequestLogDto"/> représentant les logs à traiter.
        /// </param>
        /// <returns>
        /// <c>null</c> si l'opération s'est déroulée avec succès ; sinon, le message
        /// d'erreur retourné par l'exception <see cref="MySqlException"/>.
        /// </returns>
        /// <remarks>
        /// Les étapes de traitement sont les suivantes :
        /// <list type="number">
        /// <item>
        /// <description>Analyse des logs via <see cref="LogProcessingService"/>.</description>
        /// </item>
        /// <item>
        /// <description>Extraction des nouvelles entités : sites, sessions, pages visitées et actions utilisateurs.</description>
        /// </item>
        /// <item>
        /// <description>Insertion en base de données dans une transaction unique.</description>
        /// </item>
        /// <item>
        /// <description>Validation (commit) ou annulation (rollback) en cas d'erreur.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Peut être levée si la liste <paramref name="logs"/> est nulle.
        /// </exception>
        public async Task<string?> bulkInsertLogs(List<RequestLogDto> logs, FileMonitoring fileMonitoring, int nbInvalidLog)
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

                        fileMonitoring.Speed = (int)(DateTime.UtcNow - fileMonitoring.TreatementDate).TotalMilliseconds;
                        fileMonitoring.Status = FileStatus.TREATED;
                        fileMonitoring.TreatedLogs = logs.Count;
                        fileMonitoring.SkippedLogs = nbInvalidLog;

                        _fileMonitoring.Update(fileMonitoring, connection);

                        return null;
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine("Exception : " + ex.Message.ToString());
                        transaction.Rollback();

                        fileMonitoring.Speed = (int)(DateTime.UtcNow - fileMonitoring.TreatementDate).TotalMilliseconds;
                        fileMonitoring.Status = FileStatus.FAILED;
                        fileMonitoring.TreatedLogs = 0;
                        fileMonitoring.SkippedLogs = nbInvalidLog;

                        _fileMonitoring.Update(fileMonitoring, connection);

                        return ex.Message.ToString();
                    }
                }
            }
        }

        public void InsertIntoMonitoring(FileMonitoring monitoring)
        {
            var connection = _connectionManager.CreateConnection();

            _fileMonitoring.Insert(monitoring, connection);
        }
    }
}
