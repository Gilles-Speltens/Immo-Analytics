using Common;
using Message_Parser.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Message_Parser.Services
{
    public class LogProcessingService
    {
        private Dictionary<(string sessionId, string domain), (Session session, int lastHitPage)> _ongoingSessions; //with last hitpage and the session as the value

        private int _lastHitPageId;
        private int _lastSessionId;

        private List<Site> _sites = new List<Site>();
        private Dictionary<int, Session> _sessions = new Dictionary<int, Session>();
        private List<HitPage> _hitPages = new List<HitPage>();
        private List<UserAction> _userActions = new List<UserAction>();

        public LogProcessingService(Dictionary<(string sessionId, string domain), (Session, int)> ongoingSessions, int lastHitPageId, int lastSessionId) 
        {
            _ongoingSessions = ongoingSessions;
            _lastHitPageId = lastHitPageId;
            _lastSessionId = lastSessionId;
        }

        public List<Site> GetSites() { return _sites; }
        public List<Session> GetSessions() { return _sessions.Values.ToList(); }
        public List<HitPage> GetHitPages() { return _hitPages; }
        public List<UserAction> GetUserActions() { return _userActions; }

        public void ProcessLogs(List<RequestLogDto> logs)
        {
            _sites = new List<Site>();
            _sessions = new Dictionary<int, Session>();
            _hitPages = new List<HitPage>();
            _userActions = new List<UserAction>();
            foreach (RequestLogDto log in logs)
            {
                //Ajouter le nouveau site via un Upsert.
                var domain = SiteProcessing(log);

                //Ajouter la nouvelle session uniquement si elle ne ce trouve pas dans _ongoingSessions
                Session curSession;
                bool newSession = SessionsProcessing(log, domain, out curSession);

                //Ajouter un nouvel hitpage si action == hitpage ou si l'action n'a pas de session et doit donc créer une nouvelle hitpage sur laquel se lier ou si l'action a une session mais pas de hitapage.
                int hitPageId = 0;
                bool sessionHaveHitpage = _ongoingSessions.TryGetValue((curSession.SessionId, curSession.Site), out _);
                if (log.Action == ActionsType.HITPAGE || log.SessionId == null || !sessionHaveHitpage)
                {
                    hitPageId = HitpageProcessing(log, curSession);
                }

                //Si nouvelle session créée et si la session n'est pas une session autogénéré (session-less) l'ajouter à la liste des session à garder en mémoire _ongoingSessions.
                if (log.SessionId != null && newSession)
                {
                    _ongoingSessions.Add((curSession.SessionId, curSession.Site), (curSession, hitPageId));
                }

                //Ajouter une action lier à un hitpage.
                if (log.Action != ActionsType.HITPAGE)
                {
                    ActionProcessing(log, domain, hitPageId);
                }
            }
        }

        private string SiteProcessing(RequestLogDto log)
        {
            var url = log.Url;
            if (!url.StartsWith("http"))
                url = "http://" + url;

            var domain = new Uri(url).Host.Replace("www.", "");
            var dateWhenAdded = DateTime.UtcNow;
            var certify = false;
            Site site = new Site { Domain = domain, DateWhenAdded = dateWhenAdded, Certify = certify };

            _sites.Add(site);

            return domain;
        }

        private bool SessionsProcessing(RequestLogDto log, string domain, out Session ongoingSession)
        {
            var newSession = false;

            // Si la session est reprise dans la liste des sessions encore active
            if (_ongoingSessions.TryGetValue((log.SessionId, domain), out var value))
            {
                ongoingSession = value.session;
                ongoingSession.UserId = log.UserId;
                ongoingSession.SessionEnd = log.Date;

                var key = _sessions.ContainsKey(ongoingSession.Id);

                //Si la session se trouve dans la liste des sessions pas encore insert
                if (key)
                {
                    _sessions[ongoingSession.Id] = ongoingSession;
                }
            }
            else
            {
                _lastSessionId += 1;
                ongoingSession = new Session
                {
                    Id = _lastSessionId,
                    SessionId = log.SessionId,
                    Site = domain,
                    UserId = log.UserId,
                    UserIp = log.UserIp,
                    LanguageBrowser = log.LanguageBrowser,
                    UserAgent = log.UserAgent,
                    SessionStart = log.Date,
                    SessionEnd = null
                };
                _sessions.Add(ongoingSession.Id, ongoingSession);
                newSession = true;
            }
            
            return newSession;
        }

        private int HitpageProcessing(RequestLogDto log, Session curSession)
        {
            _lastHitPageId += 1;
            var hitPageId = _lastHitPageId;
            var time = log.Date;
            var url = log.Url;
            var referrer = log.UrlReferrer;

            var hit = new HitPage { Id = _lastHitPageId, Time = time, SessionPk = curSession.Id, Url = url, Referrer = referrer };
            _hitPages.Add(hit);

            return hitPageId;
        }

        private void ActionProcessing(RequestLogDto log, string domain, int hitPageId)
        {
            var time = log.Date;
            var actionType = log.Action;
            var actionParam = log.ActionParameters;
            int pageId;
            if (_ongoingSessions.TryGetValue((log.SessionId, domain), out var value))
            {
                // Récupère la dernière hitpage de la session.
                pageId = value.lastHitPage;
            }
            else
            {
                pageId = hitPageId;
            }
            var action = new UserAction { Id = null, Time = time, PageId = pageId, ActionType = actionType, ActionParameter = actionParam };

            _userActions.Add(action);
        }
    }
}
