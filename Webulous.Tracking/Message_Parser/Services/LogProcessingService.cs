using Common;
using Message_Parser.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Message_Parser.Services
{
    /// <summary>
    /// Service responsable de la transformation des logs bruts en entités métier :
    /// - Sites
    /// - Sessions
    /// - HitPages (pages visitées)
    /// - Actions utilisateurs
    ///
    /// Il maintient également un état des sessions en cours (_ongoingSessions)
    /// afin de reconstruire correctement la navigation utilisateur.
    /// </summary>
    public class LogProcessingService
    {
        /// <summary>
        /// Sessions en cours, indexées par (SessionId, Domain).
        /// Contient :
        /// - la session en cours
        /// - l'identifiant de la dernière page visitée (HitPage)
        /// </summary>
        private Dictionary<(string sessionId, string domain), (Session session, long lastHitPage)> _ongoingSessions;

        private long _lastHitPageId;
        private long _lastSessionId;

        private List<Site> _sites = new List<Site>();
        private Dictionary<long, Session> _sessions = new Dictionary<long, Session>();
        private List<HitPage> _hitPages = new List<HitPage>();
        private List<UserAction> _userActions = new List<UserAction>();

        /// <summary>
        /// Initialise une nouvelle instance du service de traitement des logs.
        /// </summary>
        /// <param name="ongoingSessions">Sessions encore actives provenant d’un traitement précédent.</param>
        /// <param name="lastHitPageId">Dernier identifiant de HitPage utilisé.</param>
        /// <param name="lastSessionId">Dernier identifiant de Session utilisé.</param>
        public LogProcessingService(Dictionary<(string sessionId, string domain), (Session, long)> ongoingSessions, long lastHitPageId, long lastSessionId) 
        {
            _ongoingSessions = ongoingSessions;
            _lastHitPageId = lastHitPageId;
            _lastSessionId = lastSessionId;
        }

        public List<Site> GetSites() { return _sites; }
        public List<Session> GetSessions() { return _sessions.Values.ToList(); }
        public List<HitPage> GetHitPages() { return _hitPages; }
        public List<UserAction> GetUserActions() { return _userActions; }

        /// <summary>
        /// Traite une liste de logs et reconstruit les entités métier associées.
        ///
        /// Pipeline de traitement pour chaque log :
        /// 1. Détermination du site (domain)
        /// 2. Gestion de la session (création ou mise à jour)
        /// 3. Création éventuelle d’une HitPage
        /// 4. Ajout dans les sessions actives (_ongoingSessions)
        /// 5. Création d’une action utilisateur (si applicable)
        /// </summary>
        /// <param name="logs">Logs à traiter.</param>
        public void ProcessLogs(List<RequestLogDto> logs)
        {
            _sites = new List<Site>();
            _sessions = new Dictionary<long, Session>();
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
                long hitPageId = 0;
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

        /// <summary>
        /// Extrait le domaine depuis l'URL du log et crée une entité Site.
        /// </summary>
        /// <returns>Le domaine normalisé.</returns>
        private string SiteProcessing(RequestLogDto log)
        {
            var url = log.Url;
            if (url == null) return "unknown";
            if (!url.StartsWith("http"))
                url = "http://" + url;

            var domain = new Uri(url).Host.Replace("www.", "");
            var dateWhenAdded = DateTime.UtcNow;
            var certify = false;
            Site site = new Site { Domain = domain, DateWhenAdded = dateWhenAdded, Certify = certify };

            _sites.Add(site);

            return domain;
        }

        /// <summary>
        /// Gère la création ou mise à jour d’une session.
        ///
        /// Cas :
        /// - Session existante → mise à jour (UserId, SessionEnd)
        /// - Nouvelle session → création avec nouvel ID
        /// </summary>
        /// <returns>True si une nouvelle session a été créée.</returns>
        private bool SessionsProcessing(RequestLogDto log, string domain, out Session ongoingSession)
        {
            var newSession = false;

            // Si la session est reprise dans la liste des sessions encore active
            if (_ongoingSessions.TryGetValue((log.SessionId, domain), out var value))
            {
                ongoingSession = value.session;

                // Update session data based on last log
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

        /// <summary>
        /// Crée une nouvelle HitPage (page visitée) associée à une session.
        /// </summary>
        /// <returns>Identifiant de la HitPage créée.</returns>
        private long HitpageProcessing(RequestLogDto log, Session curSession)
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

        /// <summary>
        /// Crée une action utilisateur liée à une HitPage.
        ///
        /// Si une session existe déjà :
        /// → on rattache à la dernière HitPage connue
        /// Sinon :
        /// → on utilise la HitPage créée dans ce cycle
        /// </summary>
        private void ActionProcessing(RequestLogDto log, string domain, long hitPageId)
        {
            var time = log.Date;
            var actionType = log.Action;
            var actionParam = log.ActionParameters;
            long pageId;
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
