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

        private Dictionary<Session, int> _ongoingSessions = new Dictionary<Session, int>(); //with last hitpage of the session as the value
        private int _sessionExpirationTime = 20;

        private int _lastHitPageId;

        public UnitOfWork()
        {
            _db = DBConnection.Instance;
            _sessionRepo = new SessionsRepository(_db);
            _hitpageRepo = new HitPageRepository(_db);
            _userActionsRepo = new UserActionsRepository(_db);
            _siteRepo = new SiteRepository(_db);

            List<Session> tempSessions = _sessionRepo.GetAllAfterDateOrdered(DateTime.UtcNow.AddMinutes(-(_sessionExpirationTime)));
            List<int> tempHitPages = _hitpageRepo.GetLastHitPageOfSessionOredered(tempSessions);

            for (int i = 0; i < tempHitPages.Count; i++)
            {
                _ongoingSessions.Add(tempSessions[i], tempHitPages[i]);
            }

            _lastHitPageId = _hitpageRepo.GetLastId();
        }

        public async Task<bool> bulkInsertLogs(List<RequestLogDto> logs)
        {
            _db.Open();

            List<Site> sites = new List<Site>();
            List<Session> sessions = new List<Session>();
            List<HitPage> hitPage = new List<HitPage>();
            List<UserAction> userActions = new List<UserAction>();
            foreach (RequestLogDto log in logs)
            {
                var domain = Regex.Match(log.Url, @"^(?:https?:\/\/)?([^\/:?#]+)").Groups[1].Value;
                var dateWhenAdded = DateTime.UtcNow;
                var certify = false;
                Site site = new Site { Domain = domain, DateWhenAdded = dateWhenAdded, Certify = certify };

                sites.Add(site);

                int idSessionTable;
                if (!_ongoingSessions.Any(s =>
                        s.Key.SessionId == log.SessionId
                        && s.Key.Site == domain
                    ))
                {
                    //TODO UPDATE SessionsEnd
                    idSessionTable = 0;
                }
                else
                {
                    idSessionTable = _ongoingSessions.Last().Key.Id++;
                    var session = log.SessionId;
                    var siteFk = domain;
                    var userId = log.UserId;
                    var userIp = log.UserIp;
                    var language = log.LanguageBrowser;
                    var userAgent = log.UserAgent;
                    var start = log.Date;

                    var s = new Session { Id = idSessionTable, SessionId = session, Site = siteFk, UserId = userId, UserIp = userIp, LanguageBrowser = language, UserAgent = userAgent, SessionStart = start, SessionEnd = start };
                    sessions.Add(s);
                }

                var hitPageId = 0;
                if (log.Action == ActionsType.HITPAGE || log.SessionId == null)
                {
                    _lastHitPageId += 1;
                    hitPageId = _lastHitPageId;
                    var time = log.Date;
                    var url = log.Url;
                    var referrer = log.UrlReferrer;

                    var hit = new HitPage { Id = _lastHitPageId, Time = time, SessionPk = idSessionTable, Url = url, Referrer = referrer };
                    hitPage.Add(hit);

                    if (log.SessionId != null)
                    {
                        _ongoingSessions.Add(sessions.Last(), hitPageId);
                    }
                }

                if (log.Action != ActionsType.HITPAGE)
                {
                    
                }
            }
        }
    }
}
