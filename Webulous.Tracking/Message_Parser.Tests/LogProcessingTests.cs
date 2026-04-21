using Common;
using Common.UserActions;
using Message_Parser.Entities;
using Message_Parser.Services;
using System;
using System.Collections.Generic;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Message_Parser.Tests
{
    public class LogProcessingTests
    {
        private LogProcessingService CreateService(
        Dictionary<(string sessionId, string domain), (Session, int)> ongoing = null,
        int lastHitPageId = 0,
        int lastSessionId = 0)
        {
            ongoing ??= new Dictionary<(string, string), (Session, int)>();
            return new LogProcessingService(ongoing, lastHitPageId, lastSessionId);
        }

        private RequestLogDto CreateLog(
            string url = "https://www.youtube.com/watch?v=test",
            string sessionId = "ABC",
            ActionsType action = ActionsType.HITPAGE,
            DateTime? date = null)
        {
            return new RequestLogDto
            {
                Url = url,
                UrlReferrer = "https://google.com",
                SessionId = sessionId,
                UserId = "1",
                UserIp = "127.0.0.1",
                LanguageBrowser = "fr",
                UserAgent = "agent",
                Date = date ?? DateTime.UtcNow,
                Action = action,
                ActionParameters = "param"
            };
        }

        [Fact]
        public void ProcessLogs_ShouldCreateSite()
        {
            var service = CreateService();
            var logs = new List<RequestLogDto> { CreateLog() };

            service.ProcessLogs(logs);

            var sites = service.GetSites();
            Assert.Single(sites);
            Assert.Equal("youtube.com", sites[0].Domain);
        }

        [Fact]
        public void ProcessLogs_ShouldCreateSession()
        {
            var service = CreateService();
            var logs = new List<RequestLogDto> { CreateLog() };

            service.ProcessLogs(logs);

            var sessions = service.GetSessions();
            Assert.Single(sessions);
            Assert.Equal("ABC", sessions[0].SessionId);
            Assert.Equal("youtube.com", sessions[0].Site);
        }

        [Fact]
        public void ProcessLogs_ShouldCreateHitPage()
        {
            var service = CreateService();
            var logs = new List<RequestLogDto> { CreateLog() };

            service.ProcessLogs(logs);

            var hitpages = service.GetHitPages();
            Assert.Single(hitpages);
            Assert.Equal("https://www.youtube.com/watch?v=test", hitpages[0].Url);
        }

        [Fact]
        public void ProcessLogs_ShouldCreateUserAction_WhenNotHitPage()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(action: ActionsType.HITPAGE),
                CreateLog(action: ActionsType.BUTTON_CLICK)
            };

            service.ProcessLogs(logs);

            var actions = service.GetUserActions();
            Assert.Single(actions);
            Assert.Equal(ActionsType.BUTTON_CLICK, actions[0].ActionType);
        }

        [Fact]
        public void ProcessLogs_ShouldReuseExistingSession()
        {
            var ongoingSessions = new Dictionary<(string sessionId, string domain), (Session, int)>();

            var existingSession = new Session
            {
                Id = 1,
                UserIp = "127.0.0.1",
                SessionId = "ABC",
                Site = "youtube.com",
                LanguageBrowser = "fr",
                UserAgent = "Google",
                SessionStart = DateTime.UtcNow
            };

            ongoingSessions.Add(("ABC", "youtube.com"), (existingSession, 1));

            var service = new LogProcessingService(ongoingSessions, 1, 1);

            var logs = new List<RequestLogDto> { CreateLog() };

            service.ProcessLogs(logs);

            var sessions = service.GetSessions();
            Assert.Empty(sessions); // pas de nouvelle session créée
        }

        [Fact]
        public void ShouldCreateMultipleSites_WhenDifferentDomains()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog("https://youtube.com/watch"),
                CreateLog("https://github.com/test")
            };

            service.ProcessLogs(logs);

            var sites = service.GetSites();
            Assert.Equal(2, sites.Count);
        }

        [Fact]
        public void ShouldCreateMultipleSessions_WhenDifferentSessionIds()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(sessionId: "A"),
                CreateLog(sessionId: "B")
            };

            service.ProcessLogs(logs);

            var sessions = service.GetSessions();
            Assert.Equal(2, sessions.Count);
        }

        [Fact]
        public void ShouldIncrementHitPageIds()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(),
                CreateLog()
            };

            service.ProcessLogs(logs);

            var hitpages = service.GetHitPages();
            Assert.Equal(2, hitpages.Count);
            Assert.Equal(1, hitpages[0].Id);
            Assert.Equal(2, hitpages[1].Id);
        }

        [Fact]
        public void ShouldCreateActionLinkedToLastHitPage()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(action: ActionsType.HITPAGE),
                CreateLog(action: ActionsType.BUTTON_CLICK)
            };

            service.ProcessLogs(logs);

            var actions = service.GetUserActions();
            Assert.Single(actions);
            Assert.Equal(1, actions[0].PageId);
        }

        [Fact]
        public void ShouldCreateHitPage_WhenSessionIdIsNull()
        {
            var service = CreateService();

            var log = CreateLog(sessionId: null, action: ActionsType.BUTTON_CLICK);
            var logs = new List<RequestLogDto> { log };

            service.ProcessLogs(logs);

            var hitpages = service.GetHitPages();
            Assert.Single(hitpages);
        }

        [Fact]
        public void ShouldStoreReferrer()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto> { CreateLog() };

            service.ProcessLogs(logs);

            var hitpages = service.GetHitPages();
            Assert.Equal("https://google.com", hitpages[0].Referrer);
        }

        [Fact]
        public void ShouldCreateAction_WhenNotHitPage()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
        {
            CreateLog(action: ActionsType.HITPAGE),
            CreateLog(action: ActionsType.ESTATSE_BROWSING),
            CreateLog(action: ActionsType.BUTTON_CLICK)
        };

            service.ProcessLogs(logs);

            var actions = service.GetUserActions();
            Assert.Equal(2, actions.Count);
        }

        [Fact]
        public void ShouldContinueIds_FromLastValues()
        {
            var service = CreateService(lastHitPageId: 10, lastSessionId: 5);

            var logs = new List<RequestLogDto> { CreateLog() };

            service.ProcessLogs(logs);

            var sessions = service.GetSessions();
            var hitpages = service.GetHitPages();

            Assert.Equal(6, sessions[0].Id);
            Assert.Equal(11, hitpages[0].Id);
        }

        [Fact]
        public void ShouldReuseOngoingSession_AndNotCreateNewOne()
        {
            var ongoing = new Dictionary<(string, string), (Session, int)>();

            var existingSession = new Session
            {
                Id = 1,
                UserIp = "127.0.0.1",
                SessionId = "ABC",
                Site = "youtube.com",
                LanguageBrowser = "fr",
                UserAgent = "Google",
                SessionStart = DateTime.UtcNow
            };

            ongoing.Add(("ABC", "youtube.com"), (existingSession, 1));

            var service = CreateService(ongoing, 1, 1);

            var logs = new List<RequestLogDto> { CreateLog() };

            service.ProcessLogs(logs);

            var sessions = service.GetSessions();
            Assert.Empty(sessions);
        }

        [Fact]
        public void SameSession_ShouldCreateOneSession_MultipleHitPages()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(action: ActionsType.HITPAGE),
                CreateLog(action: ActionsType.HITPAGE),
                CreateLog(action: ActionsType.HITPAGE)
            };

            service.ProcessLogs(logs);

            Assert.Single(service.GetSessions());
            Assert.Equal(3, service.GetHitPages().Count);
        }

        [Fact]
        public void ActionWithoutHitPage_ShouldCreateHitPage()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(action: ActionsType.BUTTON_CLICK)
            };

            service.ProcessLogs(logs);

            Assert.Single(service.GetHitPages());
            Assert.Single(service.GetUserActions());
        }

        [Fact]
        public void MultipleActions_ShouldLinkToSameHitPage()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(action: ActionsType.HITPAGE),
                CreateLog(action: ActionsType.BUTTON_CLICK),
                CreateLog(action: ActionsType.ESTATSE_BROWSING),
                CreateLog(action: ActionsType.BUTTON_CLICK)
            };

            service.ProcessLogs(logs);

            var actions = service.GetUserActions();

            Assert.Equal(3, actions.Count);
            Assert.All(actions, a => Assert.Equal(1, a.PageId));
        }

        [Fact]
        public void SameSessionId_DifferentDomains_ShouldCreateDifferentSessions()
        {
            var service = CreateService();

            var logs = new List<RequestLogDto>
            {
                CreateLog(url: "https://youtube.com", sessionId: "ABC"),
                CreateLog(url: "https://github.com", sessionId: "ABC")
            };

            service.ProcessLogs(logs);

            Assert.Equal(2, service.GetSessions().Count);
        }

        [Fact]
        public void NewSession_ShouldBeAddedToOngoingSessions()
        {
            var ongoing = new Dictionary<(string, string), (Session, int)>();
            var service = CreateService(ongoing);

            var logs = new List<RequestLogDto>
            {
                CreateLog()
            };

            service.ProcessLogs(logs);

            Assert.Single(ongoing);
        }

        [Fact]
        public void OngoingSession_ShouldStoreLastHitPage()
        {
            var ongoing = new Dictionary<(string, string), (Session, int)>();
            var service = CreateService(ongoing);

            var logs = new List<RequestLogDto>
            {
                CreateLog(action: ActionsType.HITPAGE)
            };

            service.ProcessLogs(logs);

            var value = ongoing.Values.First();
            Assert.Equal(1, value.Item2);
        }

        [Fact]
        public void EmptyLogs_ShouldCreateNothing()
        {
            var service = CreateService();

            service.ProcessLogs(new List<RequestLogDto>());

            Assert.Empty(service.GetSites());
            Assert.Empty(service.GetSessions());
            Assert.Empty(service.GetHitPages());
            Assert.Empty(service.GetUserActions());
        }

        [Fact]
        public void ExistingSession_ShouldUpdateUserId()
        {
            var service = CreateService();

            var log1 = CreateLog();
            var log2 = CreateLog();
            log2.UserId = "99";

            var logs = new List<RequestLogDto> { log1, log2 };

            service.ProcessLogs(logs);

            var session = service.GetSessions().First();
            Assert.Equal("99", session.UserId);
        }

        [Fact]
        public void SessionStart_ShouldBeFirstLogDate()
        {
            var service = CreateService();

            var date1 = DateTime.UtcNow;
            var date2 = date1.AddMinutes(10);

            var logs = new List<RequestLogDto>
            {
                CreateLog(date: date1),
                CreateLog(date: date2)
            };

            service.ProcessLogs(logs);

            var session = service.GetSessions().First();
            Assert.Equal(date1, session.SessionStart);
        }
    }
}
